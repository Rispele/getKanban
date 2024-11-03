using Domain.Game.Days.DayEvents;

namespace Domain.Game.Days.Scenarios;

public class ScenarioItemBuilder
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