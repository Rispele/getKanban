namespace Domain.Game.Days;

public class Day
{
	public long Number { get; }

	private readonly long currentStep;
	
	public Day(long number)
	{
		Number = number;
		currentStep = 0;
	}
}