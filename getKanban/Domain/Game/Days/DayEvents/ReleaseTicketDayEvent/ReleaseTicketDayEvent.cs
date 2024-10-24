namespace Domain.Game.Days.DayEvents.ReleaseTicketDayEvent;

public class ReleaseTicketDayEvent : DayEvent
{
	public ReleaseTicketDayEvent(IReadOnlyList<string> ticketIds, int id)
		: base(DayEventType.ReleaseTicket, id)
	{
		TicketIds = ticketIds;
	}

	public IReadOnlyList<string> TicketIds { get; }
}