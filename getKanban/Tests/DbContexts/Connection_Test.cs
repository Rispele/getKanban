using Domain;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests.DbContexts;

[TestFixture]
public class Connection_Test
{
	[TestCase(null)]
	public void Connect_EnsureCreate_ShouldSuccessfullyCreate(string? connectionString)
	{
		var db = TestDomainContextProvider.Get(connectionString);
		db.Database.EnsureCreated();
	}

	[TestCase(null)]
	public void Remove_Created(string? connectionString)
	{
		var db = TestDomainContextProvider.Get(connectionString);
		db.Model
			.GetEntityTypes()
			.Select(t => t.GetTableName()!)
			.Select(DropTableSql)
			.ForEach(
				sql =>
				{
					try
					{
						db.Database.ExecuteSqlRaw(sql);
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
				});

		string DropTableSql(string tableName)
		{
			return $"""DROP TABLE public."{tableName}" CASCADE;""";
		}
	}
}