using Domain;
using Microsoft.EntityFrameworkCore;
using WebApp.DbContexts;

var db = new TeamsContext();
db.Database.EnsureCreated();

Console.ReadLine();
db.Model
	.GetEntityTypes()
	.Select(t => t.GetTableName()!)
	.Select(DropTableSql)
	.ForEach(sql => db.Database.ExecuteSqlRaw(sql));

string DropTableSql(string tableName)
{
	return $"""DROP TABLE public."{tableName}" CASCADE;""";
}