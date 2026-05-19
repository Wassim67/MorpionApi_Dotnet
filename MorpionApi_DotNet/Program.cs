using MorpionApi_DotNet.Endpoints;
using MorpionApi_DotNet.Players;
using MorpionApi_DotNet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IBotPlayer, RandomBotPlayer>();
builder.Services.AddSingleton<MorpionGameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapMorpionEndpoints();

app.Run();
