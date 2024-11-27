namespace Core.Dtos.Containers.RollDice;

public class RollDiceContainerDto : DayContainerDto
{
	public IReadOnlyList<DiceRollResultDto> DiceRollResults { get; init; }
}