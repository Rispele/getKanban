using Domain.Game.Days.DayEvents.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(WorkAnotherTeamContainerEntityTypeConfiguration))]
public class WorkAnotherTeamContainer
{
	public long Id { get; }
	
	public long DayId { get; }

	public int DiceNumber { get; }

	public int ScoresNumber { get; }

	[UsedImplicitly]
	private WorkAnotherTeamContainer()
	{
	}

	private WorkAnotherTeamContainer(
		long dayId,
		int diceNumber,
		int scoresNumber)
	{
		DayId = dayId;
		DiceNumber = diceNumber;
		ScoresNumber = scoresNumber;
	}

	internal static WorkAnotherTeamContainer CreateInstance(
		Day day,
		int diceNumber,
		int scoresNumber)
	{
		day.PostDayEvent(DayEventType.WorkAnotherTeam);

		return new WorkAnotherTeamContainer(
			day.Id,
			diceNumber,
			scoresNumber);
	}
}