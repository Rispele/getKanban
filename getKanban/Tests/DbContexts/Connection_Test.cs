using Domain;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Web.DbContexts;

namespace Tests.DbContexts;

[TestFixture]
public class Connection_Test
{
	[Test]
	public void Connect_EnsureCreate_ShouldSuccessfullyCreate()
	{
		var db = new GameSessionsContext();
		db.Database.EnsureCreated();
	}

	[Test]
	public void Remove_Created()
	{
		var db = new GameSessionsContext();
		db.Model
			.GetEntityTypes()
			.Select(t => t.GetTableName()!)
			.Select(DropTableSql)
			.ForEach(sql => db.Database.ExecuteSqlRaw(sql));

		string DropTableSql(string tableName)
		{
			return $"""DROP TABLE public."{tableName}" CASCADE;""";
		}
	}
}