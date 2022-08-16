using ProductService;
using ProductService.Configs;
using ProductService.Mapper;
using ProductService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Интеграция в DI конфигурации для сервисов
builder.Services.Configure<ProductStoreDatabaseSettings>(c =>
{
    c.ConnectionString = builder.Configuration.GetValue<string>("Mongo_ConnectionString");
    c.ProductsCollectionName = builder.Configuration.GetValue<string>("Mongo_ProductsCollectionName");
    c.DatabaseName = builder.Configuration.GetValue<string>("Mongo_DatabaseName");
});
builder.Services.Configure<KafkaConsumerSettings>(c => 
{
    c.GroupId = builder.Configuration.GetValue<string>("KafkaConsumerSettings_GroupId");
    c.BootstrapServers= builder.Configuration.GetValue<string>("KafkaConsumerSettings_BootstrapServers");
});

// Внедрение зависимости ProductContext
builder.Services.AddSingleton<ProductContext>();
// Аналогично с ProductService
builder.Services.AddSingleton<ProductService.Services.ProductService>();
// Добавление зависимости фонового сервиса KafkaConsumerService
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddTransient<IMapper, Mapper>();

// Подключение Serilog для логов
builder.Host.UseSerilog((context, config) => config
                        .WriteTo.Console());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProductServiceGrpc>();
app.Run();
