using Domain.Game.Days.DayEvents;
using Newtonsoft.Json;

namespace Domain.Game.Days.Scenarios;

public class Scenario
{
	[JsonProperty]
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
			.SelectMany(item => item.EventTypes)
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