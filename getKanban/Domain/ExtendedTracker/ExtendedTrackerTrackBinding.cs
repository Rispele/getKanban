namespace Domain.ExtendedTracker;

public record ExtendedTrackerTrackBinding(
	ExtendedTrackerTypeMemberDefinition Tracker,
	ExtendedTrackerTypeMemberDefinition Tracking)
{
	public string TrackingName => Tracking.Name;

	public bool IsUpdated(object obj)
	{
		return Tracker.Value(obj)?.Equals(true) ?? false;
	}
}