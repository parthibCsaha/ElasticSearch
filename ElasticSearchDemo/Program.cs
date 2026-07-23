using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Http.Connections;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(_ =>
{
    var settings = new ElasticsearchClientSettings(
            new Uri("https://localhost:9200"))
        .Authentication(new BasicAuthentication("elastic", "tm14aXThh7PadRnh1qyC"))
        .ServerCertificateValidationCallback(
            (sender, certificate, chain, sslPolicyErrors) => true)
        .DefaultIndex("products")
        .DisableDirectStreaming();

    return new ElasticsearchClient(settings);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ping-es", async (ElasticsearchClient client) =>
{
    var ping = await client.PingAsync();

    return ping.IsValidResponse
        ? Results.Ok("Elasticsearch Connected")
        : Results.BadRequest(ping.DebugInformation);
});

app.MapControllers();

app.Run();
