using Domain.Game.Days.DayEvents;

namespace Domain.Game.Days.Scenarios;

public class ScenarioBuilder
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

	public Scenario Build()
	{
		return new Scenario(scenario);
	}

	public static implicit operator Scenario(ScenarioBuilder builder)
	{
		return builder.Build();
	}
}