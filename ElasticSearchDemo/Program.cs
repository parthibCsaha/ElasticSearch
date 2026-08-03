using Elastic.Clients.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
