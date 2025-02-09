﻿using Domain.Game.Teams.Configurations;
using Domain.Game.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Teams;

[EntityTypeConfiguration(typeof(TeamSessionSettingsEntityTypeConfiguration))]
[PrimaryKey(nameof(Id))]
public class TeamSessionSettings
{
	public long Id { get; }

	public IReadOnlyList<Ticket> InitiallyTakenTickets { get; init; } = null!;

	public IReadOnlyList<(int dayFrom, int profitPerClient)> ProfitPerClientPerDay { get; init; } = null!;

	public int MaxDayNumber { get; init; }

	public int ReleaseCycleLength { get; init; }

	public int UpdateSprintBacklogEveryDaySince { get; init; }

	public int AnotherTeamShouldWorkSince { get; init; }

	public int ScoresAnotherTeamShouldGain { get; init; }

	public int AnalystsNumber { get; init; }

	public int ProgrammersNumber { get; init; }

	public int DefaultTestersNumber { get; init; }

	public int IncreasedTestersNumber { get; init; }

	public int IncreaseTestersNumberSince { get; init; }

	public int LockTestersSince { get; init; }

	public int LockTestersBeforeInclusive { get; init; }

	public int DayBeforeInclusiveNotReleasedTasksTakenResultsInPenalty { get; init; }

	public int ClientsPenaltyPerTaskNotReleased { get; init; }

	public int ProfitPenaltyPerClient { get; init; }

	public Dictionary<int, List<TicketDescriptor>> TicketsAppearedByDay { get; init; } = null!;

	public List<TicketDescriptor> GetTicketsAllowed(int dayNumber)
	{
		return TicketsAppearedByDay
			.Where(t => t.Key <= dayNumber)
			.SelectMany(t => t.Value)
			.ToList();
	}

	public int GetProfitPerDay(int dayNumber)
	{
		return ProfitPerClientPerDay
			.Where(t => t.dayFrom < dayNumber)
			.MaxBy(t => t.dayFrom)
			.profitPerClient;
	}

	public bool ShouldLockTesters(int dayNumber)
	{
		return LockTestersSince <= dayNumber && dayNumber <= LockTestersBeforeInclusive;
	}

	public static TeamSessionSettings Default()
	{
		return new TeamSessionSettings
		{
			InitiallyTakenTickets =
			[
				Ticket.Create(TicketDescriptors.S01.Id, 1),
				Ticket.Create(TicketDescriptors.S02.Id, 1),
				Ticket.Create(TicketDescriptors.S03.Id, 1),
				Ticket.Create(TicketDescriptors.S04.Id, 1),
				Ticket.Create(TicketDescriptors.S05.Id, 3),
				Ticket.Create(TicketDescriptors.S06.Id, 3),
				Ticket.Create(TicketDescriptors.S07.Id, 3),
				Ticket.Create(TicketDescriptors.S08.Id, 3),
				Ticket.Create(TicketDescriptors.S09.Id, 6),
				Ticket.Create(TicketDescriptors.S10.Id, 6),
				Ticket.Create(TicketDescriptors.S11.Id, 6)
			],
			ProfitPerClientPerDay =
			[
				(0, 10),
				(9, 15),
				(12, 20),
				(15, 30)
			],
			ReleaseCycleLength = 3,
			UpdateSprintBacklogEveryDaySince = 14,
			AnotherTeamShouldWorkSince = 11,
			ScoresAnotherTeamShouldGain = 15,
			AnalystsNumber = 2,
			ProgrammersNumber = 3,
			DefaultTestersNumber = 2,
			IncreasedTestersNumber = 3,
			LockTestersSince = 12,
			LockTestersBeforeInclusive = 14,
			IncreaseTestersNumberSince = 15,
			MaxDayNumber = 18,
			DayBeforeInclusiveNotReleasedTasksTakenResultsInPenalty = 9,
			ClientsPenaltyPerTaskNotReleased = 10,
			ProfitPenaltyPerClient = 50,
			TicketsAppearedByDay = new Dictionary<int, List<TicketDescriptor>>
			{
				[0] = TicketDescriptors.AllTicketDescriptors
					.Where(t => t.Id != TicketDescriptors.BusinessTask.Id)
					.ToList(),
				[16] = [TicketDescriptors.BusinessTask]
			}
		};
	}
}