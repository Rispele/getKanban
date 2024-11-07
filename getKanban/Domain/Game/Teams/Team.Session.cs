using Domain.Game.Days;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.Scenarios;
using Domain.Game.Tickets;

namespace Domain.Game.Teams;

public partial class Team
{
	private readonly TeamSessionSettings settings;

	private int currentDayNumber;
	private List<Day> days { get; } = null!;
	private int previousDayNumber => currentDayNumber - 1;

	public IReadOnlyList<Day> Days => days;
	public Day CurrentDay => days.Single(d => d.Number == currentDayNumber);
	internal Day? previousDay => days.SingleOrDefault(d => d.Number == previousDayNumber);

	public IReadOnlyList<TeamRoleUpdate> CurrentDayTeamRoleUpdates =>
		CurrentDay.UpdateTeamRolesContainer.TeamRoleUpdates;

	public RollDiceContainer? CurrentDayRollDiceContainer => CurrentDay.RollDiceContainer;

	public IReadOnlyList<UpdateCfdContainer> CfdContainers => days
		.OrderBy(d => d.Number)
		.Select(d => d.UpdateCfdContainer)
		.Where(c => c.Frozen)
		.ToList();

	public void ExecuteCommand(DayCommand command)
	{
		command.Execute(this, CurrentDay);
	}

	internal void AddNextDay()
	{
		currentDayNumber++;
		days.Add(ConfigureDay(currentDayNumber, days));
	}

	public HashSet<string> BuildTakenTickets(IReadOnlyList<Day> daysToProcess)
	{
		var takenTickets = daysToProcess.SelectMany(t => t.UpdateSprintBacklogContainer?.TicketIds ?? []);
		return settings.InitiallyTakenTickets.Concat(takenTickets).ToHashSet();
	}

	public HashSet<string> BuildReleasedTickets(IReadOnlyList<Day> daysToProcess)
	{
		return daysToProcess.SelectMany(t => t.ReleaseTicketContainer?.TicketIds ?? []).ToHashSet();
	}

	public HashSet<string> BuildTicketsInWork(IReadOnlyList<Day> daysToProcess)
	{
		return BuildTakenTickets(daysToProcess)
			.Except(BuildReleasedTickets(daysToProcess))
			.ToHashSet();
	}

	public int BuildAnotherTeamScores(List<Day> daysToProcess)
	{
		return daysToProcess.Select(d => d.WorkAnotherTeamContainer?.ScoresNumber ?? 0).Sum();
	}

	private Day ConfigureDay(int dayNumber, List<Day> daysToProcess)
	{
		var takenTickets = BuildTakenTickets(daysToProcess);
		var endOfReleaseCycle = dayNumber % settings.ReleaseCycleLength == 0;

		var shouldRelease = endOfReleaseCycle || takenTickets.Contains(TicketDescriptors.AutoRelease.Id);
		var shouldUpdateSpringBacklog = endOfReleaseCycle || dayNumber >= settings.UpdateSprintBacklogEveryDaySince;
		var anotherTeamAppeared = dayNumber > 9 &&
		                          BuildAnotherTeamScores(daysToProcess) < settings.UpdateSprintBacklogEveryDaySince;

		var (scenario, initiallyAwaitedEvents) = ConfigureScenario(
			anotherTeamAppeared,
			shouldRelease,
			shouldUpdateSpringBacklog);

		var testersNumber = dayNumber >= settings.IncreaseTestersNumberSince
			? settings.IncreasedTestersNumber
			: settings.DefaultTestersNumber;

		return new Day(
			dayNumber,
			scenario,
			initiallyAwaitedEvents,
			settings.AnalystsNumber,
			settings.ProgrammersNumber,
			testersNumber);
	}

	//TODO: пока что так, потом мб декларативно опишем
	private static (Scenario, List<DayCommandType>) ConfigureScenario(
		bool anotherTeamAppeared,
		bool shouldRelease,
		bool shouldUpdateSprintBacklog)
	{
		var scenarioBuilder = ScenarioBuilder.Create()
			.For(
				DayCommandType.WorkAnotherTeam,
				b => b
					.AwaitCommands(DayCommandType.UpdateTeamRoles, DayCommandType.RollDice)
					.RemoveAwaited(DayCommandType.WorkAnotherTeam))
			.For(DayCommandType.UpdateTeamRoles, builder => builder.ReAwaitCommand(DayCommandType.UpdateTeamRoles))
			.For(
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
				})
			.For(DayCommandType.ReleaseTickets, builders => builders.ReAwaitCommand(DayCommandType.ReleaseTickets))
			.For(DayCommandType.UpdateSprintBacklog, builders => builders.ReAwaitCommand(DayCommandType.UpdateSprintBacklog))
			.For(
				DayCommandType.UpdateCfd,
				builder => builder.ReAwaitCommand(DayCommandType.UpdateCfd).WithCondition("Released", null),
				builder => builder.ReAwaitCommand(DayCommandType.UpdateCfd).WithCondition("ToDeploy", null),
				builder => builder.ReAwaitCommand(DayCommandType.UpdateCfd).WithCondition("WithTesters", null),
				builder => builder.ReAwaitCommand(DayCommandType.UpdateCfd).WithCondition("WithProgrammers", null),
				builder => builder.ReAwaitCommand(DayCommandType.UpdateCfd).WithCondition("WithAnalysts", null),
				builder => builder.AwaitCommands(DayCommandType.UpdateCfd, DayCommandType.EndDay)
					.WithCondition("Released", ScenarioItemConditions.NotNull)
					.WithCondition("ToDeploy", ScenarioItemConditions.NotNull)
					.WithCondition("WithTesters", ScenarioItemConditions.NotNull)
					.WithCondition("WithProgrammers", ScenarioItemConditions.NotNull)
					.WithCondition("WithAnalysts", ScenarioItemConditions.NotNull)
					.RemoveAwaited(DayCommandType.UpdateCfd))
			.For(DayCommandType.EndDay, b => b);
		return (
			scenarioBuilder,
			anotherTeamAppeared
				? [DayCommandType.WorkAnotherTeam]
				: [DayCommandType.UpdateTeamRoles, DayCommandType.RollDice]
		);
	}
}