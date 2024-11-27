namespace Domain.ExtendedTracker;

public record ExtendedTrackerTrackBinding(
	ExtendedTrackerTypeMemberDefinition Tracker,
	ExtendedTrackerTypeMemberDefinition Tracking)
{
	public bool IsUpdated(object obj) => Tracker.Value(obj)?.Equals(true) ?? false;
	
	public string TrackingName => Tracking.Name;
}