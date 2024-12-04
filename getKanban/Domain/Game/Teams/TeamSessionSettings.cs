using Domain.Game.Teams.Configurations;
using Domain.Game.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Teams;

[EntityTypeConfiguration(typeof(TeamSessionSettingsEntityTypeConfiguration))]
[PrimaryKey(nameof(Id))]
public class TeamSessionSettings
{
	public long Id { get; }

	public IReadOnlyList<Ticket> InitiallyTakenTickets { get; init; } = null!;

	public int ReleaseCycleLength { get; init; }

	public int UpdateSprintBacklogEveryDaySince { get; init; }

	public int AnotherTeamShouldWorkSince { get; init; }

	public int ScoresAnotherTeamShouldGain { get; init; }

	public int AnalystsNumber { get; init; }

	public int ProgrammersNumber { get; init; }

	public int DefaultTestersNumber { get; init; }

	public int IncreasedTestersNumber { get; init; }

	public int IncreaseTestersNumberSince { get; init; }

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
			ReleaseCycleLength = 3,
			UpdateSprintBacklogEveryDaySince = 14,
			AnotherTeamShouldWorkSince = 9,
			ScoresAnotherTeamShouldGain = 15,
			AnalystsNumber = 2,
			ProgrammersNumber = 3,
			DefaultTestersNumber = 2,
			IncreasedTestersNumber = 3,
			IncreaseTestersNumberSince = 15
		};
	}
}