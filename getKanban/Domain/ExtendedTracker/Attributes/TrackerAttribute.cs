namespace Domain.Attributes;

public class TrackerAttribute : TrackAttribute
{
	public TrackerAttribute(string trackTag)
		: base(trackTag)
	{
	}
}