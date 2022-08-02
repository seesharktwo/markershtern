using Microsoft.Extensions.Options;
using UserBagMicroservice.Data.Repository;
using UserBagMicroservice.Data.Settings;
using UserBagMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Services.AddGrpc();

builder.Services.Configure<KafkaConsumerSettings>(configuration.GetSection("KafkaConsumerSettings"));

builder.Services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoDbSettings>(serviceProvider =>
    serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

builder.Services.AddSingleton<MongoRepository<UserBagMicroservice.Models.UserBag>>();

builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

app.MapGrpcService<UserBriefcaseService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
