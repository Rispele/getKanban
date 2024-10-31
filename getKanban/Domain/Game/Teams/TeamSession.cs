using Domain.DomainExceptions;
using Domain.Game.Days;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;
using Domain.Game.Tickets;

namespace Domain.Game.Teams;

public class TeamSession
{
	private readonly List<Day> days = null!;
	private readonly TeamSessionSettings settings;

	private int currentDayNumber;

	private Day CurrentDay => days[currentDayNumber - 9];
	private Day? PreviousDay => currentDayNumber - 10 < 0 ? null : days[currentDayNumber - 10];

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

	public Guid TeamId { get; }

	public long Id { get; }

	public Lazy<HashSet<string>> TicketsInWork { get; }

	public Lazy<HashSet<string>> TakenTickets { get; }

	public Lazy<HashSet<string>> ReleasedTickets { get; }

	public Lazy<int> AnotherTeamScores { get; }

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

	public void UpdateSprintBacklog(string[] ticketIds)
	{
		EnsureCanTakeTickets(ticketIds);

		CurrentDay.UpdateSprintBacklog(ticketIds);
	}

	public void UpdateCfd(
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		var previousDayCfd = PreviousDay?.UpdateCfdContainer ?? UpdateCfdContainer.None;
		var released = ReleasedTickets.Value.Count;

		var currentSumToValidate = released + readyToDeploy;
		var previousSumToValidate = previousDayCfd.Released + previousDayCfd.ReadyToDeploy;
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

		CurrentDay.UpdateCfd(
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
		CurrentDay.EndDay();

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

		var dayContext = ConfigureDayContext(
			dayNumber,
			anotherTeamAppeared,
			shouldRelease,
			shouldUpdateSpringBacklog);

		var testersNumber = currentDayNumber >= settings.IncreaseTestersNumberSince
			? settings.IncreasedTestersNumber
			: settings.DefaultTestersNumber;

		return new Day(
			Id,
			dayContext,
			settings.AnalystsNumber,
			settings.ProgrammersNumber,
			testersNumber);
	}

	//TODO: пока что так, потом мб декларативно опишем
	private static DayContext ConfigureDayContext(
		int dayNumber,
		bool anotherTeamAppeared,
		bool shouldRelease,
		bool shouldUpdateSprintBacklog)
	{
		var scenario = new Dictionary<DayEventType, List<DayEventType>>();

		if (anotherTeamAppeared)
		{
			scenario[DayEventType.WorkAnotherTeam] =
			[
				DayEventType.UpdateTeamRoles,
				DayEventType.RollDice
			];
		}

		scenario[DayEventType.UpdateTeamRoles] = [DayEventType.UpdateTeamRoles];
		if (shouldRelease && shouldUpdateSprintBacklog)
		{
			scenario[DayEventType.RollDice] = [DayEventType.ReleaseTickets];
			scenario[DayEventType.ReleaseTickets] = [DayEventType.UpdateSprintBacklog];
			scenario[DayEventType.UpdateSprintBacklog] = [DayEventType.UpdateCfd];
		}
		else if (shouldRelease)
		{
			scenario[DayEventType.RollDice] = [DayEventType.ReleaseTickets];
			scenario[DayEventType.ReleaseTickets] = [DayEventType.UpdateCfd];
		}
		else if (shouldUpdateSprintBacklog)
		{
			scenario[DayEventType.RollDice] = [DayEventType.UpdateSprintBacklog];
			scenario[DayEventType.UpdateSprintBacklog] = [DayEventType.UpdateCfd];
		}
		else
		{
			scenario[DayEventType.RollDice] = [DayEventType.UpdateCfd];
		}

		scenario[DayEventType.UpdateCfd] = [DayEventType.EndDay];

		return new DayContext(
			scenario,
			anotherTeamAppeared ? [DayEventType.WorkAnotherTeam] : [DayEventType.UpdateTeamRoles, DayEventType.RollDice]
		);
	}
}