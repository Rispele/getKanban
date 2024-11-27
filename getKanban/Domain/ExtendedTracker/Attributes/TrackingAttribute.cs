namespace Domain.Attributes;

public class TrackingAttribute : TrackAttribute
{
	public TrackingAttribute(string trackTag)
		: base(trackTag)
	{
	}
}