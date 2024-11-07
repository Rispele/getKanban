namespace Domain.Game.Days.Scenarios;

public record ScenarioItem(DayCommandType[] ToAdd, ScenarioItemCondition[] conditions, DayCommandType[] ToRemove);