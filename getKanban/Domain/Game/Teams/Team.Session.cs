using Domain.DomainExceptions;
using Domain.Game.Days;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;
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
	private Day? previousDay => days.SingleOrDefault(d => d.Number == previousDayNumber);

	public IReadOnlyList<TeamRoleUpdate> CurrentDayTeamRoleUpdates =>
		CurrentDay.UpdateTeamRolesContainer.TeamRoleUpdates;

	public RollDiceContainer? CurrentDayRollDiceContainer => CurrentDay.RollDiceContainer;

	public IReadOnlyList<UpdateCfdContainer> CfdContainers => days
		.OrderBy(d => d.Number)
		.Select(d => d.UpdateCfdContainer)
		.Where(c => c.Frozen)
		.ToList();

	public int RollDiceForAnotherTeam()
	{
		return CurrentDay.RollDiceForAnotherTeam();
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		CurrentDay.UpdateTeamRoles(from, to);
	}

	public void RollDices()
	{
		CurrentDay.RollDices();
	}

	public void ReleaseTickets(string[] ticketIds)
	{
		EnsureCanReleaseTickets(ticketIds);

		CurrentDay.ReleaseTickets(ticketIds);
	}

	public void EndOfReleaseTickets()
	{
		CurrentDay.EndOfReleaseTickets();
	}

	public void UpdateSprintBacklog(string[] ticketIds)
	{
		EnsureCanTakeTickets(ticketIds);

		CurrentDay.UpdateSprintBacklog(ticketIds);
	}

	public void EndOfUpdateSprintBacklog()
	{
		CurrentDay.EndOfUpdateSprintBacklog();
	}

	public void UpdateCfd(UpdateCfdContainerPatchType patchType, int value)
	{
		CurrentDay.UpdateCfd(patchType, value);
	}

	public void EndOfUpdateCfd()
	{
		EnsureCanEndOfUpdateCfd();

		CurrentDay.EndOfUpdateCfd();
	}

	public void EndDay()
	{
		CurrentDay.EndDay();

		currentDayNumber++;
		days.Add(ConfigureDay(currentDayNumber, days));
	}

	private void EnsureCanEndOfUpdateCfd()
	{
		var currentDayCfd = CurrentDay.UpdateCfdContainer;
		var previousDayCfd = previousDay?.UpdateCfdContainer ?? UpdateCfdContainer.None;

		if (currentDayCfd
		    is { Released: null }
		    or { ToDeploy: null }
		    or { WithTesters: null }
		    or { WithProgrammers: null }
		    or { WithAnalysts: null })
		{
			throw new DomainException("Invalid cfd arguments");
		}

		var currentSumToValidate = currentDayCfd.Released! + currentDayCfd.ToDeploy!;
		var previousSumToValidate = previousDayCfd.Released! + previousDayCfd.ToDeploy!;
		ValidateArgumentsSum(currentSumToValidate.Value, previousSumToValidate.Value);

		currentSumToValidate += currentDayCfd.WithTesters;
		previousSumToValidate += previousDayCfd.WithTesters!;
		ValidateArgumentsSum(currentSumToValidate.Value, previousSumToValidate.Value);

		currentSumToValidate += currentDayCfd.WithProgrammers;
		previousSumToValidate += previousDayCfd.WithProgrammers!;
		ValidateArgumentsSum(currentSumToValidate.Value, previousSumToValidate.Value);

		currentSumToValidate += currentDayCfd.WithAnalysts;
		previousSumToValidate += previousDayCfd.WithAnalysts!;
		ValidateArgumentsSum(currentSumToValidate.Value, previousSumToValidate.Value);

		void ValidateArgumentsSum(int currentSum, int previousSum)
		{
			if (currentSum < previousSum)
			{
				throw new DomainException("Invalid cfd arguments");
			}
		}
	}

	private void EnsureCanTakeTickets(string[] ticketIds)
	{
		if (BuildTakenTickets(days).Overlaps(ticketIds))
		{
			throw new DayActionIsProhibitedException("You cannot take already taken tickets");
		}
	}

	private void EnsureCanReleaseTickets(string[] ticketIds)
	{
		if (!BuildTicketsInWork(days).IsSupersetOf(ticketIds))
		{
			throw new DayActionIsProhibitedException("You cannot release not in work tickets");
		}
	}

	private HashSet<string> BuildTakenTickets(List<Day> daysToProcess)
	{
		var takenTickets = daysToProcess.SelectMany(t => t.UpdateSprintBacklogContainer?.TicketIds ?? []);
		return settings.InitiallyTakenTickets.Concat(takenTickets).ToHashSet();
	}

	private HashSet<string> BuildReleasedTickets(List<Day> daysToProcess)
	{
		return daysToProcess.SelectMany(t => t.ReleaseTicketContainer?.TicketIds ?? []).ToHashSet();
	}

	private HashSet<string> BuildTicketsInWork(List<Day> daysToProcess)
	{
		return BuildTakenTickets(daysToProcess)
			.Except(BuildReleasedTickets(daysToProcess))
			.ToHashSet();
	}

	private int BuildAnotherTeamScores(List<Day> daysToProcess)
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
	private static (Scenario, List<DayEventType>) ConfigureScenario(
		bool anotherTeamAppeared,
		bool shouldRelease,
		bool shouldUpdateSprintBacklog)
	{
		var scenarioBuilder = ScenarioBuilder.Create()
			.For(DayEventType.WorkAnotherTeam, DayEventType.UpdateTeamRoles, DayEventType.RollDice)
			.For(DayEventType.UpdateTeamRoles, DayEventType.UpdateTeamRoles)
			.For(
				DayEventType.RollDice,
				shouldRelease
					? [DayEventType.ReleaseTickets, DayEventType.EndOfReleaseTickets]
					: shouldUpdateSprintBacklog
						? [DayEventType.UpdateSprintBacklog, DayEventType.EndOfUpdateSprintBacklog]
						: [DayEventType.UpdateCfd])
			.For(
				DayEventType.EndOfReleaseTickets,
				shouldUpdateSprintBacklog
					? [DayEventType.UpdateSprintBacklog, DayEventType.EndOfUpdateSprintBacklog]
					: [DayEventType.UpdateCfd])
			.For(DayEventType.EndOfUpdateSprintBacklog, DayEventType.UpdateCfd)
			.For(
				DayEventType.UpdateCfd,
				builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("Released", null),
				builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("ToDeploy", null),
				builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("WithTesters", null),
				builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("WithProgrammers", null),
				builder => builder.ForEventType(DayEventType.UpdateCfd).WithCondition("WithAnalysts", null),
				builder => builder.ForEventType(DayEventType.UpdateCfd, DayEventType.EndOfUpdateCfd)
					.WithCondition("Released", ScenarioItemConditions.NotNull)
					.WithCondition("ToDeploy", ScenarioItemConditions.NotNull)
					.WithCondition("WithTesters", ScenarioItemConditions.NotNull)
					.WithCondition("WithProgrammers", ScenarioItemConditions.NotNull)
					.WithCondition("WithAnalysts", ScenarioItemConditions.NotNull))
			.For(DayEventType.EndOfUpdateCfd, DayEventType.EndDay)
			.For(DayEventType.EndDay, Array.Empty<DayEventType>());
		return (
			scenarioBuilder,
			anotherTeamAppeared ? [DayEventType.WorkAnotherTeam] : [DayEventType.UpdateTeamRoles, DayEventType.RollDice]
		);
	}
}