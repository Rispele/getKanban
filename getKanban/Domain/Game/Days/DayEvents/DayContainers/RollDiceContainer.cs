namespace Domain.Game.Days.DayEvents.DayContainers;

public class RollDiceContainer
{
	public int DayId { get; }
	public IReadOnlyList<int> AnalystsDiceNumber { get; }
	public IReadOnlyList<int> ProgrammersDiceNumber { get; }
	public IReadOnlyList<int> TestersDiceNumber { get; }
	public IReadOnlyList<int> AnalystsScores { get; }
	public IReadOnlyList<int> ProgrammersScores { get; }
	public IReadOnlyList<int> TestersScores { get; }

	private RollDiceContainer(
		int dayId,
		IReadOnlyList<int> analystsDiceNumber,
		IReadOnlyList<int> programmersDiceNumber,
		IReadOnlyList<int> testersDiceNumber,
		IReadOnlyList<int> analystsScores,
		IReadOnlyList<int> programmersScores,
		IReadOnlyList<int> testersScores)
	{
		DayId = dayId;
		AnalystsDiceNumber = analystsDiceNumber;
		ProgrammersDiceNumber = programmersDiceNumber;
		TestersDiceNumber = testersDiceNumber;
		AnalystsScores = analystsScores;
		ProgrammersScores = programmersScores;
		TestersScores = testersScores;
	}

	internal static RollDiceContainer CreateInstance(
		Day day,
		int[] analystsDiceNumber,
		int[] programmersDiceNumber,
		int[] testersDiceNumber,
		int[] analystsScores,
		int[] programmersScores,
		int[] testersScores)
	{
		day.PostDayEvent(DayEventType.RollDice);

		return new RollDiceContainer(
			day.Id,
			analystsDiceNumber,
			programmersDiceNumber,
			testersDiceNumber,
			analystsScores,
			programmersScores,
			testersScores);
	}
}