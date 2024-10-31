using Domain.Game.Teams;
using Microsoft.EntityFrameworkCore;

namespace Domain;

public class TeamDbContext : DbContext
{
	public DbSet<Team> Users { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=db;Username=usr;Password=pwd");
	}
}