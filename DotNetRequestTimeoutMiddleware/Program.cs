using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

builder.Services.AddOpenApi();
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    options.AddPolicy("quick", TimeSpan.FromSeconds(2));
    options.AddPolicy("slow", TimeSpan.FromSeconds(10));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRequestTimeouts();

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Simulates a long operation where timeout cancellation is respected.
app.MapGet("/demo/slow", async (CancellationToken cancellationToken) =>
{
    await Task.Delay(TimeSpan.FromSeconds(8), cancellationToken);
    return Results.Ok(new { message = "Finished slow work." });
})
.WithName("SlowOperation")
.WithOpenApi()
.WithRequestTimeout("quick");

// Demonstrates endpoint override with a longer timeout policy.
app.MapGet("/demo/slow-allowed", async (CancellationToken cancellationToken) =>
{
    await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken);
    return Results.Ok(new { message = "Completed within the long timeout policy." });
})
.WithName("SlowOperationAllowed")
.WithOpenApi()
.WithRequestTimeout("slow");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program { }
