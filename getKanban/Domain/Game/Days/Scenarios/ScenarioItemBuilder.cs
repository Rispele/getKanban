using Domain.Game.Days.DayEvents;

namespace Domain.Game.Days.Scenarios;

public class ScenarioItemBuilder
{
	private readonly List<DayEventType> eventTypes = new();
	private readonly List<ScenarioItemCondition> itemConditions = new();

	public ScenarioItemBuilder ForEventType(params DayEventType[] eventTypes)
	{
		this.eventTypes.AddRange(eventTypes);
		return this;
	}

	public ScenarioItemBuilder WithCondition(string parameterName, object? parameterValue)
	{
		itemConditions.Add(new ScenarioItemCondition(parameterName, parameterValue));
		return this;
	}

	public ScenarioItem Build()
	{
		if (eventTypes.Count == 0)
		{
			throw new ArgumentException("No event types defined.");
		}
		
		return new ScenarioItem(
			eventTypes.ToArray(),
			itemConditions.ToArray());
	}

	public static implicit operator ScenarioItem(ScenarioItemBuilder builder)
	{
		return builder.Build();
	}
}