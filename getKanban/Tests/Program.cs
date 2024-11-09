using Domain.Game.Days.Commands;
using Domain.Serializers;

var t = new RollDiceCommand();

Console.WriteLine(t.ToJson());

// var db = new TeamsContext();
// db.Database.EnsureCreated();
//
// Console.ReadLine();
// db.Model
// 	.GetEntityTypes()
// 	.Select(t => t.GetTableName()!)
// 	.Select(DropTableSql)
// 	.ForEach(sql => db.Database.ExecuteSqlRaw(sql));
//
// string DropTableSql(string tableName)
// {
// 	return $"""DROP TABLE public."{tableName}" CASCADE;""";
// }