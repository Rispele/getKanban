namespace Domain.Game.Tickets;

public class Ticket
{
	public string Id { get; }
	
	public int DayOfAppearanceInSprintBacklog { get; }
	
	public int? DayOfRelease { get; }

	public Ticket(string id, int dayOfAppearanceInSprintBacklog)
	{
		Id = id;
		DayOfAppearanceInSprintBacklog = dayOfAppearanceInSprintBacklog;
		DayOfRelease = null;
	}
}