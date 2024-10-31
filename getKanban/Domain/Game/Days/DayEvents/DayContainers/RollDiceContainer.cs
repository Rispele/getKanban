namespace Domain.Game.Days.DayEvents.DayContainers;

public class RollDiceContainer
{
	private RollDiceContainer(
		int dayId,
		int[] analystsDiceNumber,
		int[] programmersDiceNumber,
		int[] testersDiceNumber,
		int[] analystsScores,
		int[] programmersScores,
		int[] testersScores)
	{
		DayId = dayId;
		AnalystsDiceNumber = analystsDiceNumber;
		ProgrammersDiceNumber = programmersDiceNumber;
		TestersDiceNumber = testersDiceNumber;
		AnalystsScores = analystsScores;
		ProgrammersScores = programmersScores;
		TestersScores = testersScores;
	}

	public int DayId { get; }
	public int[] AnalystsDiceNumber { get; }
	public int[] ProgrammersDiceNumber { get; }
	public int[] TestersDiceNumber { get; }
	public int[] AnalystsScores { get; }
	public int[] ProgrammersScores { get; }
	public int[] TestersScores { get; }

	internal static RollDiceContainer CreateInstance(
		DayContext dayContext,
		int[] analystsDiceNumber,
		int[] programmersDiceNumber,
		int[] testersDiceNumber,
		int[] analystsScores,
		int[] programmersScores,
		int[] testersScores)
	{
		dayContext.PostDayEvent(DayEventType.RollDice);
		
		return new RollDiceContainer(
			dayContext.DayId,
			analystsDiceNumber,
			programmersDiceNumber,
			testersDiceNumber,
			analystsScores,
			programmersScores,
			testersScores);
	}
}