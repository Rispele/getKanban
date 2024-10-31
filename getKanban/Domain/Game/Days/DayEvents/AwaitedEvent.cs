namespace Domain.Game.Days.DayEvents;

public class AwaitedEvent
{
	public bool Removed { get; private set; }

	public DayEventType EventType { get; }

	public AwaitedEvent(DayEventType eventType)
	{
		EventType = eventType;
	}

	public void MarkRemoved()
	{
		Removed = true;
	}
}