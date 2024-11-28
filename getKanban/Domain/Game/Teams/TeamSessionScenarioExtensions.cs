using Domain.Game.Days;
using Domain.Game.Days.Scenarios;

namespace Domain.Game.Teams;

public static class TeamSessionScenarioExtensions
{
	public static ScenarioBuilder DefaultScenario(
		this ScenarioBuilder scenario,
		bool anotherTeamAppeared,
		bool shouldRelease,
		bool shouldUpdateSprintBacklog)
	{
		var scenarioBuilder = scenario
			.WithInitiallyAwaitedCommands(ConfigureInitiallyAwaitedCommands(anotherTeamAppeared))
			.For(
				DayCommandType.WorkAnotherTeam,
				b => b
					.AwaitCommands(DayCommandType.UpdateTeamRoles, DayCommandType.RollDice)
					.RemoveAwaited(DayCommandType.WorkAnotherTeam))
			.For(DayCommandType.UpdateTeamRoles, builder => builder.ReAwaitCommand(DayCommandType.UpdateTeamRoles))
			.ConfigureRollDiceCommand(shouldRelease, shouldUpdateSprintBacklog)
			.For(DayCommandType.ReleaseTickets, builders => builders.ReAwaitCommand(DayCommandType.ReleaseTickets))
			.For(
				DayCommandType.UpdateSprintBacklog,
				builders => builders.ReAwaitCommand(DayCommandType.UpdateSprintBacklog))
			.For(
				DayCommandType.UpdateCfd,
				builder => builder.ReAwaitUpdateCfdIfNull("Released"),
				builder => builder.ReAwaitUpdateCfdIfNull("ToDeploy"),
				builder => builder.ReAwaitUpdateCfdIfNull("WithTesters"),
				builder => builder.ReAwaitUpdateCfdIfNull("WithProgrammers"),
				builder => builder.ReAwaitUpdateCfdIfNull("WithAnalysts"),
				builder => builder.AwaitCommands(DayCommandType.UpdateCfd, DayCommandType.EndDay)
					.WithCondition("Released", ScenarioItemConditions.NotNull)
					.WithCondition("ToDeploy", ScenarioItemConditions.NotNull)
					.WithCondition("WithTesters", ScenarioItemConditions.NotNull)
					.WithCondition("WithProgrammers", ScenarioItemConditions.NotNull)
					.WithCondition("WithAnalysts", ScenarioItemConditions.NotNull)
					.RemoveAwaited(DayCommandType.UpdateCfd))
			.For(DayCommandType.EndDay, b => b);
		return scenarioBuilder;
	}

	private static ScenarioItemBuilder ReAwaitUpdateCfdIfNull(
		this ScenarioItemBuilder builder,
		string parameterName)
	{
		return builder.ReAwaitIfNull(DayCommandType.UpdateCfd, parameterName);
	}
	
	private static ScenarioItemBuilder ReAwaitIfNull(
		this ScenarioItemBuilder builder,
		DayCommandType type,
		string parameterName)
	{
		return builder.ReAwaitCommand(type).WithCondition(parameterName, null);
	}

	private static DayCommandType[] ConfigureInitiallyAwaitedCommands(bool anotherTeamAppeared)
	{
		return anotherTeamAppeared
			? [DayCommandType.WorkAnotherTeam]
			: [DayCommandType.UpdateTeamRoles, DayCommandType.RollDice];
	}

	private static ScenarioBuilder ConfigureRollDiceCommand(
		this ScenarioBuilder scenarioBuilder,
		bool shouldRelease,
		bool shouldUpdateSprintBacklog)
	{
		return scenarioBuilder.For(
			DayCommandType.RollDice,
			builder =>
			{
				var toAwait = new List<DayCommandType> { DayCommandType.UpdateCfd };
				if (shouldRelease)
				{
					toAwait.Add(DayCommandType.ReleaseTickets);
				}

				if (shouldUpdateSprintBacklog)
				{
					toAwait.Add(DayCommandType.UpdateSprintBacklog);
				}

				return builder
					.AwaitCommands(toAwait.ToArray())
					.RemoveAwaited(DayCommandType.RollDice, DayCommandType.UpdateTeamRoles);
			});
	}
}