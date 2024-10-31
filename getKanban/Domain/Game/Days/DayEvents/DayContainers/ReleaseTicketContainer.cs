namespace Domain.Game.Days.DayEvents.DayContainers;

public class ReleaseTicketContainer
{
	public int DayId { get; }
	public IReadOnlyList<string> TicketIds { get; }

	private ReleaseTicketContainer(int dayId, IReadOnlyList<string> ticketIds)
	{
		DayId = dayId;
		TicketIds = ticketIds;
	}

	internal static ReleaseTicketContainer CreateInstance(
		Day day,
		IReadOnlyList<string> ticketIds)
	{
		day.PostDayEvent(DayEventType.ReleaseTickets);
		return new ReleaseTicketContainer(day.Id, ticketIds);
	}
}