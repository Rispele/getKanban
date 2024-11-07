namespace Domain.Game.Days.DayHistory;

public abstract class DayHistoryItem
{
	public abstract DayCommandType ByCommand { get; }
}