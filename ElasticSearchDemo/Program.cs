using Elastic.Clients.Elasticsearch;
using ElasticSearchDemo.Extensions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ping-es", async ([FromServices] ElasticsearchClient client) =>
{
    var ping = await client.PingAsync();

    return ping.IsValidResponse
        ? Results.Ok("Elasticsearch Connected")
        : Results.BadRequest(ping.DebugInformation);
});

app.MapControllers();

app.Run();
