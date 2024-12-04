using Domain.Game.Days.Commands;

namespace Domain.Game.Days.Scenarios;

public class ScenarioBuilder
{
	private readonly Dictionary<DayCommandType, ScenarioItem[]> scenario = new();
	private DayCommandType[] initiallyAwaitedCommands;

	private ScenarioBuilder()
	{
	}

	public static ScenarioBuilder Create()
	{
		return new ScenarioBuilder();
	}

	public ScenarioBuilder For(
		DayCommandType commandType,
		params Func<ScenarioItemBuilder, ScenarioItemBuilder>[] builders)
	{
		scenario[commandType] = builders
			.Select(b => b(new ScenarioItemBuilder()).Build())
			.ToArray();
		return this;
	}

	public ScenarioBuilder WithInitiallyAwaitedCommands(params DayCommandType[] commands)
	{
		initiallyAwaitedCommands = commands;
		return this;
	}

	public Scenario Build()
	{
		return new Scenario(scenario, initiallyAwaitedCommands);
	}

	public static implicit operator Scenario(ScenarioBuilder builder)
	{
		return builder.Build();
	}
}