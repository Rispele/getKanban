using Domain.Game.Days.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers;

[EntityTypeConfiguration(typeof(WorkAnotherTeamContainerEntityTypeConfiguration))]
public class WorkAnotherTeamContainer : DayContainer
{
	public int DiceNumber { get; }

	public int ScoresNumber { get; }

	[UsedImplicitly]
	private WorkAnotherTeamContainer()
	{
	}

	private WorkAnotherTeamContainer(
		int diceNumber,
		int scoresNumber)
	{
		DiceNumber = diceNumber;
		ScoresNumber = scoresNumber;
	}

	internal static WorkAnotherTeamContainer CreateInstance(
		int diceNumber,
		int scoresNumber)
	{
		return new WorkAnotherTeamContainer(
			diceNumber,
			scoresNumber);
	}
}