namespace Core.Dtos;

public class RollDiceContainerDto : DayContainerDto
{
	public IReadOnlyList<int> AnalystsDiceNumber { get; init; } = null!;
	public IReadOnlyList<int> ProgrammersDiceNumber { get; init; } = null!;
	public IReadOnlyList<int> TestersDiceNumber { get; init; } = null!;
	public IReadOnlyList<int> AnalystsScores { get; init; } = null!;
	public IReadOnlyList<int> ProgrammersScores { get; init; } = null!;
	public IReadOnlyList<int> TestersScores { get; init; } = null!;
}