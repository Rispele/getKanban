using Domain.Attributes;
using JetBrains.Annotations;

namespace Domain.Game.Days.DayContainers;

public abstract class DayContainer
{
	public long Id { get; }

	public long Version { get; private set; }
	
	public long Timestamp { get; [UsedImplicitly] private set; }

	[Tracker("is_updated_tracker")]
	public bool IsUpdated { get; private set; }
	
	protected void SetUpdated()
	{
		if (IsUpdated)
		{
			return;
		}
		
		Version++;
		IsUpdated = true;
	}
}