namespace Domain.Game.Days.DayEvents.DayContainers;

public class WorkAnotherTeamContainer
{
	public int DayId { get; }

	public int Id { get; }

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
		DayContext dayContext,
		int diceNumber,
		int scoresNumber)
	{
		dayContext.PostDayEvent(DayEventType.WorkAnotherTeam);

		return new WorkAnotherTeamContainer(
			dayContext.DayId,
			diceNumber,
			scoresNumber);
	}
}