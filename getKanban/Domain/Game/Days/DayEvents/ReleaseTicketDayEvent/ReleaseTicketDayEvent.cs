namespace Domain.Game.Days.DayEvents.ReleaseTicketDayEvent;

public class ReleaseTicketDayEvent : DayEvent
{
	public IReadOnlyList<string> TicketIds { get; }

	public ReleaseTicketDayEvent(IReadOnlyList<string> ticketIds, int id)
		: base(DayEventType.ReleaseTickets, id)
	{
		TicketIds = ticketIds;
	}
}