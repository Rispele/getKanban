using Domain.Game.Days.Commands;

namespace Domain.Game.Days.Scenarios;

public record ScenarioItem(DayCommandType[] ToAdd, DayCommandType[] ToRemove, string? validationMethodName);