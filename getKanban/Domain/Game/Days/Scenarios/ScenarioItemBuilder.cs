using Domain.Game.Days.Commands;

namespace Domain.Game.Days.Scenarios;

public class ScenarioItemBuilder
{
	private readonly List<DayCommandType> commandTypesAwaited = [];
	private readonly List<ScenarioItemCondition> itemConditions = [];
	private readonly List<DayCommandType> commandTypesAwaitedToRemove = [];

	public ScenarioItemBuilder AwaitCommands(params DayCommandType[] eventTypes)
	{
		commandTypesAwaited.AddRange(eventTypes);
		return this;
	}

	public ScenarioItemBuilder WithCondition(string parameterName, object? parameterValue, ScenarioItemConditions? condition)
	{
		itemConditions.Add(new ScenarioItemCondition(parameterName, parameterValue, condition));
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

	public ScenarioItem Build()
	{
		return new ScenarioItem(
			commandTypesAwaited.ToArray(),
			itemConditions.ToArray(),
			commandTypesAwaitedToRemove.ToArray());
	}

	public static implicit operator ScenarioItem(ScenarioItemBuilder builder)
	{
		return builder.Build();
	}
}