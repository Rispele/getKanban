using Domain.Game.Days.Commands;
using Domain.Game.Days.Scenarios.Services;

namespace Domain.Game.Days.Scenarios;

public class ScenarioBuilder
{
	private readonly Dictionary<DayCommandType, ScenarioItem[]> scenario = [];
	private DayCommandType[] initiallyAwaitedCommands = [];
	private IScenarioService scenarioService;

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

	public ScenarioBuilder WithScenarioService(IScenarioService service)
	{
		scenarioService = service;
		return this;
	}

	public Scenario Build()
	{
		return new Scenario(scenario, initiallyAwaitedCommands, scenarioService);
	}

	public static implicit operator Scenario(ScenarioBuilder builder)
	{
		return builder.Build();
	}
}