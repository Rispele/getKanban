namespace Domain.Game;

public class DiceRoller
{
	private readonly Random random;

	public DiceRoller(Random random)
	{
		this.random = random;
	}

	public int RollDice()
	{
		return random.Next(1, 7);
	}
}