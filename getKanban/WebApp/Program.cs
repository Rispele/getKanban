using Core.Dtos.Converters;
using Core.Services.Contracts;
using Core.Services.Implementations;
using Domain.DbContexts;
using Microsoft.EntityFrameworkCore;
using WebApp.Connection;
using WebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDbContext<DomainContext>(
		optionsBuilder => optionsBuilder
			.UseNpgsql("Host=localhost;Port=5432;Database=db;Username=usr;Password=pwd")
			.UseSnakeCaseNamingConvention());
}
else
{
	var connectionStringProvider = new ConnectionStringProvider();
	var connectionString = await connectionStringProvider.GetConnectionString();
	builder.Services.AddDbContext<DomainContext>(
		optionsBuilder => optionsBuilder
			.UseNpgsql(connectionString)
			.UseSnakeCaseNamingConvention());
}

builder.Services.AddScoped<DayDtoConverter>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();
builder.Services.AddScoped<IDomainInteractionService, DomainInteractionService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
var app = builder.Build();

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
	"default",
	"{controller=Home}/{action=Index}");
app.MapRazorPages();
app.MapHub<LobbyHub>("/lobbyHub");
app.MapHub<TeamSessionHub>("/teamSessionHub");

app.Run();