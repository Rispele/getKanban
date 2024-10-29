namespace Domain.Game.Days.DayEvents;

public class AwaitedEvent
{
	public AwaitedEvent(DayEventType eventType)
	{
		EventType = eventType;
	}

	public bool Removed { get; private set; }

	public DayEventType EventType { get; }

	public void MarkRemoved()
	{
		Removed = true;
	}
}