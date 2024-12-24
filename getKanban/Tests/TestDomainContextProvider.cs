using Domain.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public static class TestDomainContextProvider
{
	public static DomainContext Get(string? connectionString = null)
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<DomainContext>();

		dbContextOptionsBuilder
			.UseNpgsql(connectionString ?? "Host=localhost;Port=5432;Database=db;Username=usr;Password=pwd")
			.UseSnakeCaseNamingConvention();
		
		return new DomainContext(dbContextOptionsBuilder.Options);
	}
}