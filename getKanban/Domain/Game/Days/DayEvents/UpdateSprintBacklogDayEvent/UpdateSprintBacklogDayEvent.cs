namespace Domain.Game.Days.DayEvents.UpdateSprintBacklogDayEvent;

public class UpdateSprintBacklogDayEvent : DayEvent
{
	private UpdateSprintBacklogDayEvent(int dayId, int id, string[] ticketIds)
		: base(dayId, id, DayEventType.UpdateSprintBacklog)
	{
		TicketIds = ticketIds;
	}

	public string[] TicketIds { get; }

	internal static void CreateInstance(DayContext dayContext, string[] ticketIds)
	{
		var @event = new UpdateSprintBacklogDayEvent(dayContext.DayId, dayContext.NextEventId, ticketIds);
		dayContext.PostDayEvent(@event);
	}
}