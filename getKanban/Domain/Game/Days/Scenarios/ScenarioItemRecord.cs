namespace Domain.Game.Days.Scenarios;

public record ScenarioItemCondition(string parameterName, object? parameterValue, ScenarioItemConditions? conditions);