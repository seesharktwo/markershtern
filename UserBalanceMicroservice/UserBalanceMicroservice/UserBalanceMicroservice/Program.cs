using UserBalanceMicroservice.Configs;
using UserBalanceMicroservice;
using UserBalanceMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("BootstrapServerKafka"));

builder.Services.AddSingleton<BalanceContext>();

builder.Services.AddSingleton<BalanceOperationService>();
builder.Services.AddHostedService<KafkaConsumerService>();


AdminTopickBuilderService.Build();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<BalanceServiceGrpc>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
