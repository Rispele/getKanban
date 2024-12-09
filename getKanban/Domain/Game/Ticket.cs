namespace Domain.Game;

public record Ticket(string id, int takingDay, int? releaseDay)
{
	public static Ticket Create(string id, int takingDay, int? releaseDay = null) => new(id, takingDay, releaseDay);
	
	public bool IsReleasedAnyDate() => releaseDay.HasValue;
	
	public bool IsReleased(int dayNumber) => releaseDay <= dayNumber;

	public bool IsInWork(int dayNumber) => !IsReleased(dayNumber) && takingDay <= dayNumber;
	
	public bool IsTaken(int dayNumber) => takingDay <= dayNumber;
}