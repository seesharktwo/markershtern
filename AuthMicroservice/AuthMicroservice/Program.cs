using AuthMicroservice;
using AuthMicroservice.Configs;
using AuthMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрация в DI Grpc
builder.Services.AddGrpc();

// Регистрация в DI конфигурации, к которой привязан MongoDBSettings
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

// Регистрация в DI UsersContext
builder.Services.AddSingleton<UsersContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AuthService>();

app.Run();
