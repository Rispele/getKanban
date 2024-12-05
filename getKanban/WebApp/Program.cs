using Core.DbContexts;
using Core.Dtos;
using Core.Dtos.Converters;
using Core.Helpers;
using Core.Services.Contracts;
using Core.Services.Implementations;
using Domain.DbContexts;
using WebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();


builder.Services.AddScoped<DomainContext>();
builder.Services.AddScoped<ConnectionsContext>();
builder.Services.AddScoped<DayDtoConverter>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

// builder.Services.AddTransient<HtmlPageHandler>();

// var connection = builder.Configuration.GetConnectionString("DefaultConnection");

var app = builder.Build();

// var loggerFactory = app.Services.GetService<ILoggerFactory>();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.MapControllers();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}");
app.MapRazorPages();
app.MapHub<LobbyHub>("/lobbyHub");

app.Run();
