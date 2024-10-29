namespace Domain.Game.Days.DayEvents.ReleaseTicketDayEvent;

public class ReleaseTicketDayEvent : DayEvent
{
	private ReleaseTicketDayEvent(int dayId, int id, IReadOnlyList<string> ticketIds)
		: base(dayId, id, DayEventType.ReleaseTickets)
	{
		TicketIds = ticketIds;
	}

	public IReadOnlyList<string> TicketIds { get; }

	internal static void CreateInstance(
		DayContext dayContext,
		IReadOnlyList<string> ticketIds)
	{
		var @event = new ReleaseTicketDayEvent(dayContext.DayId, dayContext.NextEventId, ticketIds);
		dayContext.PostDayEvent(@event);
	}
}