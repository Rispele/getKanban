using Domain.Game.Days.Commands;

namespace Domain.Game.Days.Scenarios;

public class ScenarioItemBuilder
{
	private readonly List<DayCommandType> commandTypesAwaited = [];
	private readonly List<DayCommandType> commandTypesAwaitedToRemove = [];
	private string? validationMethodName;

	public ScenarioItemBuilder AwaitCommands(params DayCommandType[] eventTypes)
	{
		commandTypesAwaited.AddRange(eventTypes);
		return this;
	}

	public ScenarioItemBuilder RemoveAwaited(params DayCommandType[] eventTypes)
	{
		commandTypesAwaitedToRemove.AddRange(eventTypes);
		return this;
	}

	public ScenarioItemBuilder ReAwaitCommand(DayCommandType commandType)
	{
		commandTypesAwaited.Add(commandType);
		commandTypesAwaitedToRemove.Add(commandType);
		return this;
	}

	public ScenarioItemBuilder WithValidationMethod(string validationMethod)
	{
		validationMethodName = validationMethod;
		return this;
	}

	public ScenarioItem Build()
	{
		return new ScenarioItem(
			commandTypesAwaited.ToArray(),
			commandTypesAwaitedToRemove.ToArray(),
			validationMethodName);
	}

	public static implicit operator ScenarioItem(ScenarioItemBuilder builder)
	{
		return builder.Build();
	}
}