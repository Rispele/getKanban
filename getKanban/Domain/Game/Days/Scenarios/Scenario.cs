using Newtonsoft.Json;

namespace Domain.Game.Days.Scenarios;

public class Scenario
{
	[JsonProperty] private readonly Dictionary<DayCommandType, ScenarioItem[]> scenario;

	public Scenario(Dictionary<DayCommandType, ScenarioItem[]> scenario)
	{
		this.scenario = scenario;
	}

	public (DayCommandType[] toAdd, DayCommandType[] toRemove) GetNextAwaited(
		DayCommandType dayCommandType,
		object? parameters)
	{
		var items = scenario[dayCommandType];
		var itemsMatched = items
			.Where(item => MatchConditions(parameters, item.conditions))
			.ToArray();
		return (
			itemsMatched.SelectMany(item => item.ToAdd).Distinct().ToArray(),
			itemsMatched.SelectMany(item => item.ToRemove).Distinct().ToArray());
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