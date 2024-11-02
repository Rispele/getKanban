using Domain;
using Microsoft.EntityFrameworkCore;

var db = new TeamDbContext();
db.Database.EnsureCreated();

Console.ReadLine();
db.Model
	.GetEntityTypes()
	.Select(t => t.GetTableName()!)
	.Select(DropTableSql)
	.ForEach(sql => db.Database.ExecuteSqlRaw(sql));

// db.Database.ExecuteSqlRaw("select tablename from pg_tables where schemaname = 'public'");
//
// db.Database.ExecuteSqlRaw(@"
// DROP TABLE public.""AwaitedEvent"" CASCADE;
// DROP TABLE public.""Day"" CASCADE;
// DROP TABLE public.""Participant"" CASCADE;
// DROP TABLE public.""ParticipantTeam"" CASCADE;
// DROP TABLE public.""ReleaseTicketContainer"" CASCADE;
// DROP TABLE public.""RollDiceContainer"" CASCADE;
// DROP TABLE public.""TeamRoleUpdate"" CASCADE;
// DROP TABLE public.""TeamSession"" CASCADE;
// DROP TABLE public.""TeamSessionSettings"" CASCADE;
// DROP TABLE public.""UpdateCfdContainer"" CASCADE;
// DROP TABLE public.""UpdateSprintBacklogContainer"" CASCADE;
// DROP TABLE public.""UpdateTeamRolesContainer"" CASCADE;
// DROP TABLE public.""Users"" CASCADE;
// DROP TABLE public.""WorkAnotherTeamContainer"" CASCADE;");

string DropTableSql(string tableName) => $"""DROP TABLE public."{tableName}" CASCADE;""";