using Domain.Game;
using Domain.Game.Teams;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Core.DbContexts;

public class DomainContext : DbContext
{
	public DbSet<GameSession> GameSessions { get; set; }
	public DbSet<Team> Teams { get; set; }
	public DbSet<User> Users { get; set; }
	
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder
			.UseNpgsql("Host=localhost;Port=5432;Database=db;Username=usr;Password=pwd")
			.UseSnakeCaseNamingConvention();
	}
}