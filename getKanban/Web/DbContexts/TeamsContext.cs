using Domain.Game.Teams;
using Microsoft.EntityFrameworkCore;

namespace Web.DbContexts;

public class TeamsContext : DbContext
{
	public DbSet<Team> Teams { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder
			.UseNpgsql("Host=localhost;Port=5432;Database=db;Username=usr;Password=pwd")
			.UseSnakeCaseNamingConvention();
	}
}