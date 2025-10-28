using LogPort.Api.Endpoints;
using LogPort.Core.Interface;
using LogPort.Core.Models;
using LogPort.ElasticSearch;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Configuration.AddEnvironmentVariables(prefix: "LOGPORT_");
var logPortConfig = LogPortConfig.LoadFromEnvironment();
builder.Configuration.GetSection("LOGPORT").Bind(logPortConfig);
builder.Services.AddSingleton(logPortConfig);
builder.Services.AddSingleton(ElasticClientFactory.Create(logPortConfig));
builder.Services.AddScoped<ILogRepository, ElasticLogRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapLogEndpoints();

app.Run();