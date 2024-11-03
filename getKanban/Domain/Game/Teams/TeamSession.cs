using Domain.DomainExceptions;
using Domain.Game.Days;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;
using Domain.Game.Days.Scenarios;
using Domain.Game.Teams.Configurations;
using Domain.Game.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Teams;

[EntityTypeConfiguration(typeof(TeamSessionEntityTypeConfiguration))]
public class TeamSession
{
	private readonly List<Day> days = null!;
	private readonly TeamSessionSettings settings;

	private int currentDayNumber;

	private Day currentDay => days[currentDayNumber - 9];
	private Day? previousDay => currentDayNumber - 10 < 0 ? null : days[currentDayNumber - 10];

	public Guid TeamId { get; }

	public long Id { get; }

	public Lazy<HashSet<string>> TicketsInWork { get; }

	public Lazy<HashSet<string>> TakenTickets { get; }

	public Lazy<HashSet<string>> ReleasedTickets { get; }

	public Lazy<int> AnotherTeamScores { get; }

	private TeamSession()
	{
		settings = TeamSessionSettings.Default();
		TakenTickets = new Lazy<HashSet<string>>(BuildTakenTickets);
		TicketsInWork = new Lazy<HashSet<string>>(BuildTicketsInWork);
		ReleasedTickets = new Lazy<HashSet<string>>(BuildReleasedTickets);
		AnotherTeamScores = new Lazy<int>(BuildAnotherTeamScores);
	}

	public TeamSession(Guid teamId)
		: this()
	{
		TeamId = teamId;

		currentDayNumber = 9;
		days = [];
	}

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

	public void UpdateCfd(
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		var previousDayCfd = previousDay?.UpdateCfdContainer ?? UpdateCfdContainer.None;
		var released = ReleasedTickets.Value.Count;

		var currentSumToValidate = released + readyToDeploy;
		var previousSumToValidate = previousDayCfd.Released + previousDayCfd.ToDeploy;
		ValidateArgumentsSum(currentSumToValidate, previousSumToValidate);

		currentSumToValidate += withTesters;
		previousSumToValidate += previousDayCfd.WithTesters;
		ValidateArgumentsSum(currentSumToValidate, previousSumToValidate);

		currentSumToValidate += withProgrammers;
		previousSumToValidate += previousDayCfd.WithProgrammers;
		ValidateArgumentsSum(currentSumToValidate, previousSumToValidate);

		currentSumToValidate += withAnalysts;
		previousSumToValidate += previousDayCfd.WithAnalysts;
		ValidateArgumentsSum(currentSumToValidate, previousSumToValidate);

		currentDay.UpdateCfd(
			released,
			readyToDeploy,
			withTesters,
			withProgrammers,
			withAnalysts);

		void ValidateArgumentsSum(int currentSum, int previousSum)
		{
			if (currentSum < previousSum)
			{
				throw new DomainException("Invalid cfd arguments");
			}
		}
	}

	public void EndDay()
	{
		currentDay.EndDay();

		currentDayNumber++;
		days.Add(ConfigureDay(currentDayNumber));
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
		var endOfReleaseCycle = currentDayNumber % settings.ReleaseCycleLength == 0;

		var shouldRelease = endOfReleaseCycle || takenTickets.Contains(TicketDescriptors.AutoRelease.Id);
		var shouldUpdateSpringBacklog = endOfReleaseCycle
		                                || currentDayNumber >= settings.UpdateSprintBacklogEveryDaySince;
		var anotherTeamAppeared = currentDayNumber > 9
		                          && AnotherTeamScores.Value < settings.UpdateSprintBacklogEveryDaySince;

		var (scenario, initiallyAwaitedEvents) = ConfigureScenario(
			anotherTeamAppeared,
			shouldRelease,
			shouldUpdateSpringBacklog);

		var testersNumber = currentDayNumber >= settings.IncreaseTestersNumberSince
			? settings.IncreasedTestersNumber
			: settings.DefaultTestersNumber;

		return new Day(
			Id,
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
				builder => builder.ForEventType(DayEventType.EndOfUpdateCfd)
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