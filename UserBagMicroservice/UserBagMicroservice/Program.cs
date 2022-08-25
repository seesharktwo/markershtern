using Microsoft.Extensions.Options;
using UserBagMicroservice.Data.Interceptors;
using UserBagMicroservice.Data.Repository;
using UserBagMicroservice.Data.Settings;
using UserBagMicroservice.KafkaServices;
using UserBagMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServerLoggingInterceptor>();
}); ;

builder.Services.Configure<MongoDbSettings>(configuration.GetSection("Database"));
builder.Services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IMongoDbSettings>(serviceProvider =>
    serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

builder.Services.AddSingleton<MongoRepository<UserBagMicroservice.Models.UserBag>>();
builder.Services.AddSingleton<MongoRepository<UserBagMicroservice.Models.Product>>();

builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddSingleton<UserBagOperationService>();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

var topicService = new AdminTopicBuilderService(app);
topicService.TopicsBuildAsync();

app.MapGrpcService<UserBagServiceGrpc>();

app.Run();
