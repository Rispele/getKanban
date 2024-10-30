namespace Domain.Game.Days.DayEvents.WorkAnotherTeamDayEvent;

public class WorkAnotherTeamDayEvent : DayEvent
{
	private WorkAnotherTeamDayEvent(
		int dayId,
		int id,
		int diceNumber,
		int scoresNumber)
		: base(dayId, id, DayEventType.WorkAnotherTeam)
	{
		DiceNumber = diceNumber;
		ScoresNumber = scoresNumber;
	}

	public int DiceNumber { get; }

	public int ScoresNumber { get; }

	internal static void CreateInstance(
		DayContext dayContext,
		int diceNumber,
		int scoresNumber)
	{
		var @event = new WorkAnotherTeamDayEvent(
			dayContext.DayId,
			dayContext.NextEventId,
			diceNumber,
			scoresNumber);
		dayContext.PostDayEvent(@event);
	}
}