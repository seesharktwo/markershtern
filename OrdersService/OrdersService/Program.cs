using Microsoft.Extensions.Options;
using OrdersService.Data.Repository;
using OrdersService.Data.Settings;
using OrdersService.Services;
using OrdersService.Services.KafkaSettings;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

builder.Services.Configure<KafkaSettings>(configuration.GetSection("KafkaSettings"));

builder.Services.AddSingleton<IMongoDbSettings>(serviceProvider =>
    serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddSingleton<IKafkaSettings>(serviceProvider =>
    serviceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value);

builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

builder.Services.AddTransient(typeof(OrderOperationService));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<GreeterService>();
app.MapGrpcService<OrderServiceGrpc>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
