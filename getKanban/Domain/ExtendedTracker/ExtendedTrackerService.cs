using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Domain.ExtendedTracker;

public class ExtendedTrackerService
{
	public ExtendedTrackerRegistry Registry { get; }

	public ExtendedTrackerService(ExtendedTrackerRegistry registry)
	{
		Registry = registry;
	}

	public void SetModifiedIfUpdated(EntityEntryEventArgs args)
	{
		if (!Registry.TryGetTrackedMemberName(args.Entry.Entity, out var memberNames))
		{
			return;
		}

		memberNames!.ForEach(memberName => args.Entry.Property(memberName).IsModified = true);
	}
}