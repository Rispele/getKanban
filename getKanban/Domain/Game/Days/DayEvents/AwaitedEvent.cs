using Domain.Game.Days.DayEvents.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents;

[EntityTypeConfiguration(typeof(AwaitedEventEntityTypeConfiguration))]
public class AwaitedEvent
{
	public long Id { get; }

	public bool Removed { get; private set; }

	public DayEventType EventType { get; }


	[UsedImplicitly]
	public AwaitedEvent()
	{
	}

	public AwaitedEvent(DayEventType eventType)
	{
		EventType = eventType;
	}

	public void MarkRemoved()
	{
		Removed = true;
	}
}