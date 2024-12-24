namespace Domain.Game;

public record Ticket(string id, int takingDay, int? releaseDay)
{
	public static Ticket Create(string id, int takingDay, int? releaseDay = null)
	{
		return new Ticket(id, takingDay, releaseDay);
	}

	public bool IsReleasedAnyDate()
	{
		return releaseDay.HasValue;
	}

	public bool IsReleased(int dayNumber)
	{
		return releaseDay <= dayNumber;
	}

	public bool IsInWork(int dayNumber)
	{
		return !IsReleased(dayNumber) && takingDay <= dayNumber;
	}

	public bool IsTaken(int dayNumber)
	{
		return takingDay <= dayNumber;
	}
}