using Domain.DomainExceptions;
using Domain.Game.Days;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;
using Domain.Game.Days.Scenarios;
using Domain.Game.Tickets;

namespace Domain.Game.Teams;

public partial class Team
{
	private readonly List<Day> days = null!;
	private readonly TeamSessionSettings settings;

	private int currentDayNumber;
	private int previousDayNumber => currentDayNumber - 1;

	private Day currentDay => days.Single(d => d.Number == currentDayNumber);
	private Day? previousDay => days.SingleOrDefault(d => d.Number == previousDayNumber);

	public IReadOnlyList<TeamRoleUpdate> CurrentDayTeamRoleUpdates => currentDay.UpdateTeamRolesContainer.TeamRoleUpdates;
	
	public RollDiceContainer? CurrentDayRollDiceContainer => currentDay.RollDiceContainer;
	
	public IReadOnlyList<UpdateCfdContainer> CfdContainers => days
		.OrderBy(d => d.Number)
		.Select(d => d.UpdateCfdContainer)
		.Where(c => c.Frozen)
		.ToList();
	
	public Lazy<HashSet<string>> TicketsInWork { get; }

	public Lazy<HashSet<string>> TakenTickets { get; }

	public Lazy<HashSet<string>> ReleasedTickets { get; }

	public Lazy<int> AnotherTeamScores { get; }

	public int RollDiceForAnotherTeam()
	{
		return currentDay.RollDiceForAnotherTeam();
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		currentDay.UpdateTeamRoles(from, to);
	}

	public void RollDices()
	{
		currentDay.RollDices();
	}

	public void ReleaseTickets(string[] ticketIds)
	{
		EnsureCanReleaseTickets(ticketIds);

		currentDay.ReleaseTickets(ticketIds);
	}

	public void UpdateSprintBacklog(string[] ticketIds)
	{
		EnsureCanTakeTickets(ticketIds);

		currentDay.UpdateSprintBacklog(ticketIds);
	}

	public void UpdateCfd(UpdateCfdContainerPatchType patchType, int value)
	{
		currentDay.UpdateCfd(patchType, value);
	}

	public void EndOfUpdateCfd()
	{
		EnsureCanEndOfUpdateCfd();

		currentDay.EndOfUpdateCfd();
	}

	public void EndDay()
	{
		currentDay.EndDay();

		currentDayNumber++;
		days.Add(ConfigureDay(currentDayNumber));
	}

	private void EnsureCanEndOfUpdateCfd()
	{
		var currentDayCfd = currentDay.UpdateCfdContainer;
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
		if (TakenTickets.Value.Overlaps(ticketIds))
		{
			throw new DayActionIsProhibitedException("You cannot take already taken tickets");
		}
	}

	private void EnsureCanReleaseTickets(string[] ticketIds)
	{
		if (!TicketsInWork.Value.IsSupersetOf(ticketIds))
		{
			throw new DayActionIsProhibitedException("You cannot release not in work tickets");
		}
	}

	private HashSet<string> BuildTakenTickets()
	{
		var takenTickets = days.SelectMany(t => t.UpdateSprintBacklogContainer?.TicketIds ?? []);
		return settings.InitiallyTakenTickets.Concat(takenTickets).ToHashSet();
	}

	private HashSet<string> BuildReleasedTickets()
	{
		return days.SelectMany(t => t.ReleaseTicketContainer?.TicketIds ?? []).ToHashSet();
	}

	private HashSet<string> BuildTicketsInWork()
	{
		return TakenTickets.Value
			.Except(days.SelectMany(t => t.ReleaseTicketContainer?.TicketIds ?? []))
			.ToHashSet();
	}

	private int BuildAnotherTeamScores()
	{
		return days.Select(d => d.WorkAnotherTeamContainer?.ScoresNumber ?? 0).Sum();
	}

	private Day ConfigureDay(int dayNumber)
	{
		var takenTickets = TakenTickets.Value;
		var endOfReleaseCycle = dayNumber % settings.ReleaseCycleLength == 0;

		var shouldRelease = endOfReleaseCycle || takenTickets.Contains(TicketDescriptors.AutoRelease.Id);
		var shouldUpdateSpringBacklog = endOfReleaseCycle || dayNumber >= settings.UpdateSprintBacklogEveryDaySince;
		var anotherTeamAppeared = dayNumber > 9 && AnotherTeamScores.Value < settings.UpdateSprintBacklogEveryDaySince;

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
					? DayEventType.ReleaseTickets
					: shouldUpdateSprintBacklog
						? DayEventType.UpdateSprintBacklog
						: DayEventType.UpdateCfd)
			.For(
				DayEventType.ReleaseTickets,
				shouldUpdateSprintBacklog ? DayEventType.UpdateSprintBacklog : DayEventType.UpdateCfd)
			.For(DayEventType.UpdateSprintBacklog, DayEventType.UpdateCfd)
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