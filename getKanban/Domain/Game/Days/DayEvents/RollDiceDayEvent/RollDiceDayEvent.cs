namespace Domain.Game.Days.DayEvents.RollDiceDayEvent;

public class RollDiceDayEvent : DayEvent
{
	public int[] AnalystsDiceNumber { get; }
	public int[] ProgrammersDiceNumber { get; }
	public int[] TestersDiceNumber { get; }
	public int[] AnalystsScores { get; }
	public int[] ProgrammersScores { get; }
	public int[] TestersScores { get; }

	public RollDiceDayEvent(
		int[] analystsDiceNumber,
		int[] programmersDiceNumber,
		int[] testersDiceNumber,
		int[] analystsScores,
		int[] programmersScores,
		int[] testersScores,
		int id)
		: base(DayEventType.RollDice, id)
	{
		AnalystsDiceNumber = analystsDiceNumber;
		ProgrammersDiceNumber = programmersDiceNumber;
		TestersDiceNumber = testersDiceNumber;
		AnalystsScores = analystsScores;
		ProgrammersScores = programmersScores;
		TestersScores = testersScores;
	}
}