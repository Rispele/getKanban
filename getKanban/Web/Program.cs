using Domain.Game;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<HtmlPageHandler>();

var app = builder.Build();

app.MapControllers();

app.MapGet("/", () => "Hello World!");
app.MapGet("/get", async (context) =>
{
	await context.Response.WriteAsync(app.Services.GetRequiredService<HtmlPageHandler>().GetHtmlPageContent());
});
app.MapGet("/roll", () => new DiceRoller(new Random()).RollDice());
app.MapGet("/add/{count}", (int count) =>
{
	var result = "";
	for (int i = 0; i < count; i++)
	{
		result += "Team " + (i + 1) + "\n";
	}
	return result;
});

app.Run();

public class HtmlPageHandler
{
	public string GetHtmlPageContent()
	{
		return File.ReadAllText("Pages/index.html");
	}
}