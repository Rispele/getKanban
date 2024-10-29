namespace Domain.Game.Days.DayEvents.RollDiceDayEvent;

public class RollDiceDayEvent : DayEvent
{
	private RollDiceDayEvent(
		int dayId,
		int id,
		int[] analystsDiceNumber,
		int[] programmersDiceNumber,
		int[] testersDiceNumber,
		int[] analystsScores,
		int[] programmersScores,
		int[] testersScores)
		: base(dayId, id, DayEventType.RollDice)
	{
		AnalystsDiceNumber = analystsDiceNumber;
		ProgrammersDiceNumber = programmersDiceNumber;
		TestersDiceNumber = testersDiceNumber;
		AnalystsScores = analystsScores;
		ProgrammersScores = programmersScores;
		TestersScores = testersScores;
	}

	public int[] AnalystsDiceNumber { get; }
	public int[] ProgrammersDiceNumber { get; }
	public int[] TestersDiceNumber { get; }
	public int[] AnalystsScores { get; }
	public int[] ProgrammersScores { get; }
	public int[] TestersScores { get; }

	internal static void CreateInstance(
		DayContext dayContext,
		int[] analystsDiceNumber,
		int[] programmersDiceNumber,
		int[] testersDiceNumber,
		int[] analystsScores,
		int[] programmersScores,
		int[] testersScores)
	{
		var @event = new RollDiceDayEvent(
			dayContext.DayId,
			dayContext.NextEventId,
			analystsDiceNumber,
			programmersDiceNumber,
			testersDiceNumber,
			analystsScores,
			programmersScores,
			testersScores);
		dayContext.PostDayEvent(@event);
	}
}