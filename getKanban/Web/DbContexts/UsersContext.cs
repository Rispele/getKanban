using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Web.DbContexts;

public class UsersContext : DbContext
{
	public DbSet<User> Users { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder
			.UseNpgsql("Host=localhost;Port=5432;Database=db;Username=usr;Password=pwd")
			.UseSnakeCaseNamingConvention();
	}
}