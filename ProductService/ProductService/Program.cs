using ProductService;
using ProductService.Configs;
using ProductService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Интеграция в DI конфигурации для сервисов
builder.Services.Configure<ProductStoreDatabaseSettings>(
    builder.Configuration.GetSection("ProductStoreDatabase"));
builder.Services.Configure<KafkaConsumerSettings>(
    builder.Configuration.GetSection("BootstrapServerKafka"));

// Внедрение зависимости ProductContext
builder.Services.AddSingleton<ProductContext>();
// Аналогично с ProductService
builder.Services.AddSingleton<ProductService.Services.ProductService>();
// Добавление зависимости фонового сервиса KafkaConsumerService
builder.Services.AddHostedService<KafkaConsumerService>();

// Подключение Serilog для логов
builder.Host.UseSerilog((context, config) => config
                        .WriteTo.Console());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProductServiceGrpc>();

app.Run();
