using ProductService;
using ProductService.Configs;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
// Интеграция в DI конфигурации для сервисов
builder.Services.Configure<ProductStoreDatabaseSettings>(
    builder.Configuration.GetSection("ProductStoreDatabase"));
builder.Services.Configure<KafkaConsumerSettings>(
    builder.Configuration.GetSection("BootstrapServerKafka"));
// Внедрение зависимости ProductContext
builder.Services.AddSingleton<ProductContext>();
builder.Services.AddSingleton<ProductService.Services.ProductService>();
builder.Services.AddHostedService<KafkaConsumerService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProductServiceGrpc>();

app.Run();
