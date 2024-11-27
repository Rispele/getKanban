using Domain.Game.Days.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers.RollDice;

[EntityTypeConfiguration(typeof(RollDiceContainerEntityTypeConfiguration))]
public class RollDiceContainer : DayContainer
{
	public readonly IReadOnlyList<DiceRollResult> DiceRollResults = null!;

	[UsedImplicitly]
	private RollDiceContainer()
	{
	}

	private RollDiceContainer(IEnumerable<DiceRollResult> diceRollResults)
	{
		DiceRollResults = diceRollResults.ToList();
	}

	internal static RollDiceContainer CreateInstance(IEnumerable<DiceRollResult> diceRollResults)
	{
		return new RollDiceContainer(diceRollResults);
	}
}