using Domain.Game.Days.DayEvents.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(RollDiceContainerEntityTypeConfiguration))]
public class RollDiceContainer
{
	public long Id { get; }
	public long DayId { get; }
	public IReadOnlyList<int> AnalystsDiceNumber { get; } = null!;
	public IReadOnlyList<int> ProgrammersDiceNumber { get; } = null!;
	public IReadOnlyList<int> TestersDiceNumber { get; } = null!;
	public IReadOnlyList<int> AnalystsScores { get; } = null!;
	public IReadOnlyList<int> ProgrammersScores { get; } = null!;
	public IReadOnlyList<int> TestersScores { get; } = null!;

	[UsedImplicitly]
	private RollDiceContainer()
	{
	}

	private RollDiceContainer(
		long dayId,
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