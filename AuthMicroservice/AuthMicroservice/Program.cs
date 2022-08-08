using AuthMicroservice;
using AuthMicroservice.Configs;
using AuthMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Registering gRPC in DI
builder.Services.AddGrpc();

// Registering with DI a configuration instance to which the MongoDBSettings section of the appsettings.json file is bound.
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

// Registering the UsersContext class in DI to support constructor injection in consuming classes.
builder.Services.AddSingleton<UsersContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AuthService>();

app.Run();
