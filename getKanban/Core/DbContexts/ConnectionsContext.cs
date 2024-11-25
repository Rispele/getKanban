using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.DbContexts;

public class ConnectionsContext : DbContext
{
	public DbSet<HubConnection> HubConnections { get; set; }
	
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder
			.UseNpgsql("Host=localhost;Port=5432;Database=connections;Username=usr;Password=pwd")
			.UseSnakeCaseNamingConvention();
	}
}