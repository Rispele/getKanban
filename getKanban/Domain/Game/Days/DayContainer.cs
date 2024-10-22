namespace Domain.Game.Days;

public class DayContainer
{
	private long currentDay;
	
	private readonly List<Day> days;

	public DayContainer()
	{
		currentDay = 9;
		days = new List<Day>();
	}
}