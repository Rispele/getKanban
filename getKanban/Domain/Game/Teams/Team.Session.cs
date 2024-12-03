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
	private readonly TeamSessionSettings settings;

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
		days.Single(x => x.Number == currentDayNumber).Status = DayStatus.Finished;
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

	public bool IsCurrentDayCfdValid()
	{
		return CurrentDay.IsCfdValid(PreviousDay?.UpdateCfdContainer ?? UpdateCfdContainer.None);
	}

	private Day ConfigureDay(int dayNumber, List<Day> daysToProcess)
	{
		var takenTickets = BuildTakenTickets(daysToProcess);
		var endOfReleaseCycle = dayNumber % settings.ReleaseCycleLength == 0;

		var shouldRelease = endOfReleaseCycle || takenTickets.Contains(TicketDescriptors.AutoRelease.Id);
		var shouldUpdateSpringBacklog = endOfReleaseCycle || dayNumber >= settings.UpdateSprintBacklogEveryDaySince;
		var anotherTeamAppeared = dayNumber > 9 &&
		                          BuildAnotherTeamScores(daysToProcess) < settings.UpdateSprintBacklogEveryDaySince;

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