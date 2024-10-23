namespace Domain.Game.Days.DayEvents.WorkAnotherTeamDayEvent;

public class WorkAnotherTeamDayEvent : DayEvent
{
	public WorkAnotherTeamDayEvent(int diceNumber, int scoresNumber, int id)
		: base(DayEventType.WorkAnotherTeam, id)
	{
		DiceNumber = diceNumber;
		ScoresNumber = scoresNumber;
	}

	public int DiceNumber { get; }

	public int ScoresNumber { get; }
}