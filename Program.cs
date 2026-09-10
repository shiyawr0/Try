using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseStaticFiles(); // Serves static files out of wwwroot

// In-memory data stores for files and high scores
var portfolioFiles = new List<object>();
var leaderboard = new List<object>();

// --- API Endpoints ---
app.MapGet("/api/portfolio", () => Results.Ok(portfolioFiles));
app.MapPost("/api/portfolio", async (HttpRequest request) => {
    var data = await request.ReadFromJsonAsync<List<object>>();
    if (data != null) {
        portfolioFiles = data;
    }
    return Results.Ok(new { status = "Secure Archive Updated" });
});

app.MapGet("/api/leaderboard", () => Results.Ok(leaderboard));
app.MapPost("/api/leaderboard", async (HttpRequest request) => {
    var scoreData = await request.ReadFromJsonAsync<ScoreEntry>();
    if (scoreData != null) {
        // Remove existing entry if present, then add and re-sort
        leaderboard.RemoveAll(x => x is ScoreEntry se && se.Name == scoreData.Name);
        leaderboard.Add(scoreData);
        leaderboard.Sort((a, b) => ((ScoreEntry)b).Score.CompareTo(((ScoreEntry)a).Score));
        
        // Keep top 5
        if (leaderboard.Count > 5) {
            leaderboard = leaderboard.GetRange(0, 5);
        }
    }
    return Results.Ok(new { status = "Leaderboard Updated" });
});

app.Run();

record ScoreEntry(string Name, int Score);

