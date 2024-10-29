namespace Domain.Game.Days.DayEvents.UpdateSprintBacklogDayEvent;

public class UpdateSprintBacklogDayEvent : DayEvent
{
	public string[] TicketIds { get; }

	public UpdateSprintBacklogDayEvent(string[] ticketIds, int id)
		: base(DayEventType.UpdateSprintBacklog, id)
	{
		TicketIds = ticketIds;
	}
}