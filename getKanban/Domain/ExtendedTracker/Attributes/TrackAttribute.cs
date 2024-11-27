namespace Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public abstract class TrackAttribute : Attribute
{
	public string TrackTag { get; init; }

	protected TrackAttribute(string trackTag)
	{
		TrackTag = trackTag;
	}
}