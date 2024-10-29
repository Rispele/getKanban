namespace Domain.Game.Days.DayEvents.EndDayDayEvent;

public class EndDayDayEvent : DayEvent
{
	private EndDayDayEvent(int dayId, int id)
		: base(dayId, id, DayEventType.EndDay)
	{
	}

	internal static void CreateInstance(DayContext dayContext)
	{
		var @event = new EndDayDayEvent(dayContext.DayId, dayContext.NextEventId);
		dayContext.PostDayEvent(@event);
	}
}