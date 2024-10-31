namespace Domain.Game.Days.DayEvents.DayContainers;

public class WorkAnotherTeamContainer
{
	public int DayId { get; }

	public int DiceNumber { get; }

	public int ScoresNumber { get; }

	private WorkAnotherTeamContainer(
		int dayId,
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