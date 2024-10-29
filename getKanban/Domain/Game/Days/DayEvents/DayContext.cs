namespace Domain.Game.Days.DayEvents;

public class DayContext
{
	private readonly List<AwaitedEvent> awaitedEvents;
	private readonly List<DayEvent> events;

	private int LastEventId => events.Max(t => t.Id);

	public DayContext(params AwaitedEvent[] awaitedEvents)
	{
	}
}