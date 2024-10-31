namespace Domain.Game.Days.DayEvents;

public class AwaitedEvent
{
	public int DayId { get; }

	public int Id { get; }

	public bool Removed { get; private set; }

	public DayEventType EventType { get; }

	public AwaitedEvent(int dayId, DayEventType eventType)
	{
		DayId = dayId;
		EventType = eventType;
	}

	public void MarkRemoved()
	{
		Removed = true;
	}
}