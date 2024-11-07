using JetBrains.Annotations;

namespace Domain.Game.Days.DayContainers;

public abstract class DayContainer
{
	public long Id { get; }

	public long Version { get; protected set; }
	
	public long Timestamp { get; [UsedImplicitly] private set; }
}