using Domain.Game.Teams.Configurations;
using Domain.Game.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Teams;

[EntityTypeConfiguration(typeof(TeamSessionSettingsEntityTypeConfiguration))]
[PrimaryKey(nameof(Id))]
public class TeamSessionSettings
{
	public long Id { get; }

	public IReadOnlyList<string> InitiallyTakenTickets { get; set; } = null!;

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
				TicketDescriptors.S01.Id,
				TicketDescriptors.S02.Id,
				TicketDescriptors.S03.Id,
				TicketDescriptors.S04.Id,
				TicketDescriptors.S05.Id,
				TicketDescriptors.S06.Id,
				TicketDescriptors.S07.Id,
				TicketDescriptors.S08.Id,
				TicketDescriptors.S09.Id,
				TicketDescriptors.S10.Id,
				TicketDescriptors.S11.Id
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