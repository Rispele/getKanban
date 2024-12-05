using Domain.Game.Days;
using Domain.Game.Days.Commands;
using Domain.Game.Days.Scenarios;
using Domain.Game.Days.Scenarios.Services;

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
			.WithScenarioService(new DefaultScenarioService())
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
				builders => builders
					.ReAwaitCommand(DayCommandType.UpdateCfd)
					.WithValidationMethod("IsCfdNotValid"),
				builder => builder
					.AwaitCommands(DayCommandType.UpdateCfd, DayCommandType.EndDay)
					.RemoveAwaited(DayCommandType.UpdateCfd)
					.WithValidationMethod("IsCfdValid"))
			.For(DayCommandType.EndDay, b => b);
		return scenarioBuilder;
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