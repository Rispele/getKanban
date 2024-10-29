namespace Domain.Game.Days.DayEvents;

public abstract class DayEvent
{
	public DayEventType Type { get; }

	public int Id { get; }

	public bool IsRemoved { get; private set; }

	protected DayEvent(DayEventType type, int id)
	{
		Type = type;
		Id = id;
	}

	public void MarkRemoved()
	{
		IsRemoved = true;
	}
}