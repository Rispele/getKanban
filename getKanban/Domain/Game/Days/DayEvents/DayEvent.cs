namespace Domain.Game.Days.DayEvents;

public abstract class DayEvent
{
	protected DayEvent(DayEventType type, int id)
	{
		Type = type;
		Id = id;
	}

	public DayEventType Type { get; }

	public int Id { get; }

	public bool IsRemoved { get; private set; }

	public void MarkRemoved()
	{
		IsRemoved = true;
	}
}