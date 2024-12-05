﻿using Domain.DomainExceptions;
using Domain.Game.Days;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.TeamMembers;
using Domain.Game.Days.Scenarios;
using Domain.Game.Tickets;

namespace Domain.Game.Teams;

public partial class Team
{
	public readonly TeamSessionSettings Settings = null!;

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

	public IReadOnlyList<AwaitedCommands> CurrentlyAwaitedCommands => CurrentDay.CurrentlyAwaitedCommands;

	internal Day? PreviousDay => days.SingleOrDefault(d => d.Number == currentDayNumber - 1);

	public IReadOnlyList<TeamMember> CurrentDayTeamRoleUpdates => CurrentDay.TeamMembersContainer.TeamMembers;

	public IReadOnlyList<UpdateCfdContainer> CfdContainers => days
		.OrderBy(d => d.Number)
		.Select(d => d.UpdateCfdContainer)
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

		return Settings.InitiallyTakenTickets
			.Select(
				t => releasedTickets.TryGetValue(t.id, out var releasedDayNumber)
					? Ticket.Create(t.id, t.takingDay, releasedDayNumber)
					: Ticket.Create(t.id, t.takingDay))
			.Concat(takenTickets).ToHashSet();
	}

	public bool IsTicketDeadlineNotExceeded(string ticketId, int deadlineInclusive)
	{
		var ticket = BuildTakenTickets().SingleOrDefault(t => t.id == ticketId);
		if (ticket == null || ticket.IsInWork(deadlineInclusive))
		{
			return currentDayNumber <= deadlineInclusive;
		}

		return ticket.releaseDay!.Value <= deadlineInclusive;
	}

	public int BuildAnotherTeamScores()
	{
		return BuildAnotherTeamScores(days);
	}

	public bool IsCurrentDayCfdValid()
	{
		return CurrentDay.IsCfdValid(PreviousDay?.UpdateCfdContainer ?? UpdateCfdContainer.None);
	}

	internal int BuildAnotherTeamScores(IReadOnlyCollection<Day> daysToProcess)
	{
		return daysToProcess.Select(d => d.WorkAnotherTeamContainer?.ScoresNumber ?? 0).Sum();
	}

	internal HashSet<string> GetTakenTicketIds(IReadOnlyList<Day> daysToProcess)
	{
		return Settings.InitiallyTakenTickets.Select(t => t.id)
			.Concat(daysToProcess.SelectMany(d => d.UpdateSprintBacklogContainer.TicketIds))
			.ToHashSet();
	}

	internal HashSet<string> GetTicketsInWorkIds(IReadOnlyList<Day> daysToProcess)
	{
		var takenTickets = GetTakenTicketIds(daysToProcess);
		takenTickets.ExceptWith(GetReleasedTicketIds(daysToProcess));
		return takenTickets;
	}

	internal HashSet<string> GetReleasedTicketIds(IReadOnlyList<Day> daysToProcess)
	{
		return daysToProcess.SelectMany(d => d.ReleaseTicketContainer.TicketIds).ToHashSet();
	}

	private Day ConfigureDay(int dayNumber, List<Day> daysToProcess)
	{
		var takenTickets = GetTakenTicketIds(daysToProcess);
		var releasedTickets = GetReleasedTicketIds(daysToProcess);
		var endOfReleaseCycle = dayNumber % Settings.ReleaseCycleLength == 0;

		var isReleaseDay = endOfReleaseCycle || releasedTickets.Contains(TicketDescriptors.AutoRelease.Id);
		var somethingToReleaseImmediately =
			takenTickets.Any(t => TicketDescriptors.GetByTicketId(t).CanBeReleasedImmediately);
		var shouldUpdateSpringBacklog = endOfReleaseCycle || dayNumber >= Settings.UpdateSprintBacklogEveryDaySince;
		var anotherTeamAppeared = dayNumber > Settings.AnotherTeamShouldWorkSince
		                       && BuildAnotherTeamScores(daysToProcess) < Settings.ScoresAnotherTeamShouldGain;

		var scenario = ScenarioBuilder.Create()
			.DefaultScenario(
				anotherTeamAppeared,
				shouldRelease: isReleaseDay || somethingToReleaseImmediately,
				shouldUpdateSpringBacklog)
			.Build();

		var testersNumber = dayNumber >= Settings.IncreaseTestersNumberSince
			? Settings.IncreasedTestersNumber
			: Settings.DefaultTestersNumber;

		var daySettings = new DaySettings
		{
			Number = dayNumber,

			AnalystsCount = Settings.AnalystsNumber,
			ProgrammersCount = Settings.ProgrammersNumber,
			TestersCount = testersNumber,
			
			CanReleaseNotImmediately = isReleaseDay,

			ProfitPerClient = Settings.GetProfitPerDay(dayNumber)
		};

		return new Day(daySettings, scenario);
	}
}