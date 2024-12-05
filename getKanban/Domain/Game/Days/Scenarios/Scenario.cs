using System.Reflection;
using Domain.Game.Days.Commands;
using Domain.Game.Days.Scenarios.Services;
using Newtonsoft.Json;

namespace Domain.Game.Days.Scenarios;

public class Scenario
{
	[JsonProperty] private readonly Dictionary<DayCommandType, ScenarioItem[]> scenario;

	[JsonProperty] public DayCommandType[] InitiallyAwaitedCommands { get; init; }

	[JsonIgnore] private readonly IScenarioService scenarioService;

	public Scenario(
		Dictionary<DayCommandType, ScenarioItem[]> scenario,
		DayCommandType[] initiallyAwaitedCommands,
		IScenarioService scenarioService)
	{
		InitiallyAwaitedCommands = initiallyAwaitedCommands;
		this.scenario = scenario;
		this.scenarioService = scenarioService;
	}

	public Scenario CopyWithService(IScenarioService service)
	{
		return new Scenario(scenario, InitiallyAwaitedCommands, service);
	}

	public (DayCommandType[] toAdd, DayCommandType[] toRemove) GetNextAwaited(
		DayCommandType dayCommandType,
		params object?[] parameters)
	{
		var items = scenario[dayCommandType];
		var itemsMatched = items
			.Where(item => MatchValidationMethod(item.validationMethodName, parameters))
			.ToArray();
		return (
			itemsMatched.SelectMany(item => item.ToAdd).Distinct().ToArray(),
			itemsMatched.SelectMany(item => item.ToRemove).Distinct().ToArray());
	}

	private bool MatchValidationMethod(string? validationMethodName, params object?[] parameters)
	{
		if (validationMethodName == null)
		{
			return true;
		}

		var method = scenarioService.GetType().GetMethod(validationMethodName);
		if (method == null)
		{
			throw new ArgumentException($"Method {validationMethodName} not found");
		}

		try
		{
			var result = method.Invoke(scenarioService, parameters);

			if (result == null)
			{
				return false;
			}

			return (bool)result;
		}
		catch (TargetInvocationException)
		{
			throw;
		}
		catch
		{
			return false;
		}
	}
}