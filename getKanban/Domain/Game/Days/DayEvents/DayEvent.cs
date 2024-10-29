namespace Domain.Game.Days.DayEvents;

public abstract class DayEvent
{
	protected DayEvent(int dayId, int id, DayEventType type)
	{
		DayId = dayId;
		Id = id;
		Type = type;
	}

	public int DayId { get; }

	public int Id { get; }

	public DayEventType Type { get; }

	public bool Removed { get; private set; }

	public void MarkRemoved()
	{
		Removed = true;
	}
}