namespace Domain.Game;

public record Ticket(string id, int takingDay, int? releaseDay)
{
	public static Ticket Create(string id, int takingDay, int? releaseDay = null) => new(id, takingDay, releaseDay);
	
	public bool IsReleased() => releaseDay.HasValue;

	public bool InWork() => !IsReleased();
}