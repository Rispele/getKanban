using Domain.DomainExceptions;
using Domain.Game.Days;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.RollDice;
using Domain.Game.Days.DayContainers.TeamMembers;
using Domain.Game.Days.Scenarios;
using Domain.Game.Tickets;

namespace Domain.Game.Teams;

public partial class Team
{
	private readonly TeamSessionSettings settings = null!;

	private int currentDayNumber;
	private List<Day> days { get; } = null!;

	public IReadOnlyList<Day> Days => days;

	public Day CurrentDay
	{
		get
		{
			if (days.IsNullOrEmpty())
			{
				throw new DomainException("Game session is not initialized");
			}

			return days.Single(d => d.Number == currentDayNumber);
		}
	}

	internal Day? PreviousDay => days.SingleOrDefault(d => d.Number == currentDayNumber - 1);

	public IReadOnlyList<TeamMember> CurrentDayTeamRoleUpdates => CurrentDay.TeamMembersContainer.TeamMembers;

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

	public HashSet<Ticket> BuildTakenTickets()
	{
		var releasedTickets = days
			.SelectMany(d => d.ReleaseTicketContainer.TicketIds.Select(t => (ticketId: t, dayNumber: d.Number)))
			.ToDictionary(t => t.ticketId, t => t.dayNumber);

		var takenTickets = days
			.SelectMany(
				d => d.UpdateSprintBacklogContainer.TicketIds
					.Select(
						t => releasedTickets.TryGetValue(t, out var releasedDayNumber)
							? Ticket.Create(t, d.Number, releasedDayNumber)
							: Ticket.Create(t, d.Number)));

		return settings.InitiallyTakenTickets
			.Select(
				t => releasedTickets.TryGetValue(t.id, out var releasedDayNumber)
					? Ticket.Create(t.id, t.takingDay, releasedDayNumber)
					: Ticket.Create(t.id, t.takingDay))
			.Concat(takenTickets).ToHashSet();
	}

	public int BuildAnotherTeamScores(List<Day> daysToProcess)
	{
		return daysToProcess.Select(d => d.WorkAnotherTeamContainer?.ScoresNumber ?? 0).Sum();
	}

	public bool IsCurrentDayCfdValid()
	{
		return CurrentDay.IsCfdValid(PreviousDay?.UpdateCfdContainer ?? UpdateCfdContainer.None);
	}

	internal HashSet<string> GetTakenTicketIds(IReadOnlyList<Day> daysToProcess)
	{
		return settings.InitiallyTakenTickets.Select(t => t.id)
			.Concat(daysToProcess.SelectMany(d => d.UpdateSprintBacklogContainer.TicketIds))
			.ToHashSet();
	}

	internal HashSet<string> GetTicketsInWorkIds(IReadOnlyList<Day> daysToProcess)
	{
		var takenTickets = GetTakenTicketIds(daysToProcess);
		takenTickets.ExceptWith(daysToProcess.SelectMany(d => d.ReleaseTicketContainer.TicketIds));
		return takenTickets;
	}

	private Day ConfigureDay(int dayNumber, List<Day> daysToProcess)
	{
		var takenTickets = GetTakenTicketIds(daysToProcess);
		var endOfReleaseCycle = dayNumber % settings.ReleaseCycleLength == 0;

		var shouldRelease = endOfReleaseCycle || takenTickets.Contains(TicketDescriptors.AutoRelease.Id);
		var shouldUpdateSpringBacklog = endOfReleaseCycle || dayNumber >= settings.UpdateSprintBacklogEveryDaySince;
		var anotherTeamAppeared = dayNumber > settings.AnotherTeamShouldWorkSince
		                       && BuildAnotherTeamScores(daysToProcess) < settings.ScoresAnotherTeamShouldGain;

		var scenario = ScenarioBuilder.Create()
			.DefaultScenario(anotherTeamAppeared, shouldRelease, shouldUpdateSpringBacklog)
			.Build();

		var testersNumber = dayNumber >= settings.IncreaseTestersNumberSince
			? settings.IncreasedTestersNumber
			: settings.DefaultTestersNumber;

		return new Day(
			dayNumber,
			scenario,
			settings.AnalystsNumber,
			settings.ProgrammersNumber,
			testersNumber);
	}
}