using Elections.Api.Core;
using Elections.Api.Logic;
using Microsoft.AspNetCore.Cors.Infrastructure;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConfigSvc>();
bool isQa = builder.Configuration.GetValue<bool>("isQa");
Console.WriteLine($"mode isQa: {isQa}");

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddCors();
builder.Services.AddSingleton<ICorsPolicyProvider, InfraCorsPolicyProvider>();

// Mongo Configuration
MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));

var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
var defaultId = isDocker ? "host.docker.internal" : "localhost";
var mongoClient = new MongoDB.Driver.MongoClient(builder.Configuration.GetConnectionString("Mongo") ?? $"mongodb://{defaultId}:27017");
builder.Services.AddSingleton<IMongoDatabase>(mongoClient.GetDatabase("ElectionsDB"));
builder.Services.AddSingleton<UserLogic>();
builder.Services.AddSingleton<SurveyLogic>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// app ---------------------------------------
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || isQa)
{
    app.MapOpenApi();
}
app.UseResponseCompression();

if (isQa)
{
    app.UseCors("qa");
}
else
{
    app.UseCors();
}

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
