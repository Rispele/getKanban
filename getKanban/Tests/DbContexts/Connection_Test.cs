using Core.DbContexts;
using Domain;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests.DbContexts;

[TestFixture]
public class Connection_Test
{
	[Test]
	public void Connect_EnsureCreate_ShouldSuccessfullyCreate()
	{
		var users = new UsersContext();
		users.Database.EnsureCreated();
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