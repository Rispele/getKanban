using Domain.Game.Days.DayEvents;

namespace Domain.Game.Days.Scenarios;

public record ScenarioItem(DayEventType[] EventType, ScenarioItemCondition[] conditions);