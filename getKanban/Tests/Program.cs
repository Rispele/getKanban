using Domain.Game.Days.DayEvents;
using Domain.Game.Days.Scenarios;

var (s, _) = ConfigureScenario(true, true, true);
var updateCfdContainer = new UpdateCfdContainer();
var a1 = s.GetNextAwaited(DayEventType.UpdateCfd, updateCfdContainer).Single();
updateCfdContainer.Released = 0;
var a2 = s.GetNextAwaited(DayEventType.UpdateCfd, updateCfdContainer).Single();
updateCfdContainer.ToDeploy = 0;
var a3 = s.GetNextAwaited(DayEventType.UpdateCfd, updateCfdContainer).Single();
updateCfdContainer.WithTesters = 0;
var a4 = s.GetNextAwaited(DayEventType.UpdateCfd, updateCfdContainer).Single();
updateCfdContainer.WithProgrammers = 0;
var a5 = s.GetNextAwaited(DayEventType.UpdateCfd, updateCfdContainer).Single();
updateCfdContainer.WithAnalysts = 0;
var a6 = s.GetNextAwaited(DayEventType.UpdateCfd, updateCfdContainer).Single();
Console.WriteLine(a1);
Console.WriteLine(a2);
Console.WriteLine(a3);
Console.WriteLine(a4);
Console.WriteLine(a5);
Console.WriteLine(a6);

static (Scenario, List<DayEventType>) ConfigureScenario(
	bool anotherTeamAppeared,
	bool shouldRelease,
	bool shouldUpdateSprintBacklog)
{
	var scenarioBuilder = ScenarioBuilder.Create()
		.For(DayEventType.WorkAnotherTeam, DayEventType.UpdateTeamRoles, DayEventType.RollDice)
		.For(DayEventType.UpdateTeamRoles, DayEventType.UpdateTeamRoles)
		.For(
			DayEventType.RollDice,
			shouldRelease
				? DayEventType.ReleaseTickets
				: shouldUpdateSprintBacklog
					? DayEventType.UpdateSprintBacklog
					: DayEventType.UpdateCfd)
		.For(
			DayEventType.ReleaseTickets,
			shouldUpdateSprintBacklog ? DayEventType.UpdateSprintBacklog : DayEventType.UpdateCfd)
		.For(DayEventType.UpdateSprintBacklog, DayEventType.UpdateCfd)
		.For(
			DayEventType.UpdateCfd,
			builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("Released", null),
			builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("ToDeploy", null),
			builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("WithTesters", null),
			builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("WithProgrammers", null),
			builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("WithAnalysts", null),
			builder => builder.ForEventType(DayEventType.EndOfUpdateCfd)
				.WithCondition("Released", ScenarioItemConditions.NotNull)
				.WithCondition("ToDeploy", ScenarioItemConditions.NotNull)
				.WithCondition("WithTesters", ScenarioItemConditions.NotNull)
				.WithCondition("WithProgrammers", ScenarioItemConditions.NotNull)
				.WithCondition("WithAnalysts", ScenarioItemConditions.NotNull))
		.For(DayEventType.EndOfUpdateCfd, DayEventType.EndDay)
		.For(DayEventType.EndDay, Array.Empty<DayEventType>());
	return (
		scenarioBuilder,
		anotherTeamAppeared ? [DayEventType.WorkAnotherTeam] : [DayEventType.UpdateTeamRoles, DayEventType.RollDice]
	);
}

public class UpdateCfdContainer
{
	public int? Released { get; set; }
	public int? ToDeploy { get; set; }
	public int? WithTesters { get; set; }
	public int? WithProgrammers { get; set; }
	public int? WithAnalysts { get; set; }
}


// var db = new TeamDbContext();
// db.Database.EnsureCreated();
//
// Console.ReadLine();
// db.Model
// 	.GetEntityTypes()
// 	.Select(t => t.GetTableName()!)
// 	.Select(DropTableSql)
// 	.ForEach(sql => db.Database.ExecuteSqlRaw(sql));
//
// string DropTableSql(string tableName) => $"""DROP TABLE public."{tableName}" CASCADE;""";