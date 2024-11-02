using System.Reflection;
using Domain.Game.Days;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;

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

public enum ScenarioItemConditions
{
	NotNull
}

internal record ScenarioItemCondition(string parameterName, object? parameterValue);

internal record ScenarioItem(DayEventType EventType, ScenarioItemCondition[] conditions);

internal class Scenario
{
	private readonly Dictionary<DayEventType, ScenarioItem[]> scenario;

	public Scenario(Dictionary<DayEventType, ScenarioItem[]> scenario)
	{
		this.scenario = scenario;
	}

	public IEnumerable<DayEventType> GetNextAwaited(DayEventType dayEventType, object? parameters)
	{
		var items = scenario[dayEventType];
		return items
			.Where(item => MatchConditions(parameters, item.conditions))
			.Select(item => item.EventType)
			.Distinct(); //TODO (d.smirnov): сделать залупку, ака OR для ScenarioItemCondition чтобы не делать этот убогий дистинкт, когда для нескольких вариантов под операцией ИЛИ нужно писать несколько ScenarioItems. Опять же, не уверен что буду этим заниматься
	}

	private bool MatchConditions(object? parameters, ScenarioItemCondition[] conditions)
	{
		if (parameters is null)
		{
			return conditions.Length == 0;
		}

		return conditions.All(c => MatchCondition(parameters, c));
	}

	private bool MatchCondition(object parameters, ScenarioItemCondition condition)
	{
		var property = parameters.GetType().GetProperty(condition.parameterName);
		if (property is not null)
		{
			return ValueMatch(property.GetValue(parameters));
		}

		var field = parameters.GetType().GetField(condition.parameterName);
		if (field is not null)
		{
			return ValueMatch(field.GetValue(parameters));
		}

		return false;

		bool ValueMatch(object? value)
		{
			if (condition.parameterValue is ScenarioItemConditions.NotNull)
			{
				return value is not null;
			}
			
			if (value is null)
			{
				return condition.parameterValue is null;
			}

			return value.Equals(condition.parameterValue);
		}
	}
}

internal class ScenarioItemBuilder
{
	private readonly List<ScenarioItemCondition> itemConditions = new();
	private DayEventType? eventType;

	public ScenarioItemBuilder ForEventType(DayEventType eventType)
	{
		this.eventType = eventType;
		return this;
	}

	public ScenarioItemBuilder WithCondition(string parameterName, object? parameterValue)
	{
		itemConditions.Add(new ScenarioItemCondition(parameterName, parameterValue));
		return this;
	}

	public ScenarioItem Build()
	{
		return new ScenarioItem(eventType ?? throw new ArgumentNullException(), itemConditions.ToArray());
	}

	public static implicit operator ScenarioItem(ScenarioItemBuilder builder)
	{
		return builder.Build();
	}
}

internal class ScenarioBuilder
{
	private readonly Dictionary<DayEventType, ScenarioItem[]> scenario = new();

	private ScenarioBuilder()
	{
	}

	public static ScenarioBuilder Create()
	{
		return new ScenarioBuilder();
	}

	public ScenarioBuilder For(
		DayEventType eventType,
		params Func<ScenarioItemBuilder, ScenarioItemBuilder>[] builders)
	{
		scenario[eventType] = builders
			.Select(b => b(new ScenarioItemBuilder()).Build())
			.ToArray();
		return this;
	}

	public ScenarioBuilder For(
		DayEventType eventType,
		params DayEventType[] eventTypes)
	{
		scenario[eventType] = eventTypes
			.Select(b => new ScenarioItem(b, []))
			.ToArray();
		return this;
	}
	
	public Scenario Build() => new Scenario(scenario);
	
	public static implicit operator Scenario(ScenarioBuilder builder) => builder.Build();
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