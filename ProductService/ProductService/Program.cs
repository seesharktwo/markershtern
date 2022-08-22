using ProductService;
using ProductService.Configs;
using ProductService.KafkaServices;
using ProductService.Mapper;
using ProductService.Services;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// intagration in DI configs
builder.Services.Configure<ProductStoreDatabaseSettings>(builder.Configuration.GetSection("MongoSettings"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

builder.Services.AddSingleton<ProductContext>();
builder.Services.AddSingleton<ProductService.Services.ProductService>();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Host.UseSerilog((context, config) => config
                        .WriteTo.Console());

var app = builder.Build();

var topickService = new AdminTopickBuilderService(app);
topickService.TopicsBuildAsync();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProductServiceGrpc>();
app.Run();
