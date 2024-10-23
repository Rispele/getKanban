using Domain.Game.Days;

namespace Domain.Game.Teams;

public class TeamSession
{
	private readonly int currentDayNumber;
	private readonly List<Day> days;
	private readonly TeamSessionSettings sessionSettings;

	public TeamSession()
	{
		sessionSettings = new TeamSessionSettings();
		currentDayNumber = 9;
		days = new List<Day>();
	}

	private Day CurrentDay => days[currentDayNumber - 9];
}