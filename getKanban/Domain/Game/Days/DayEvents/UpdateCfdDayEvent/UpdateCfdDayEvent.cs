namespace Domain.Game.Days.DayEvents.UpdateCfdDayEvent;

public class UpdateCfdDayEvent : DayEvent
{
	public int Released { get; }
	public int ReadyToDeploy { get; }
	public int WithTesters { get; }
	public int WithProgrammers { get; }
	public int WithAnalysts { get; }

	public UpdateCfdDayEvent(
		int released,
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts,
		int id)
		: base(DayEventType.UpdateCfd, id)
	{
		Released = released;
		ReadyToDeploy = readyToDeploy;
		WithTesters = withTesters;
		WithProgrammers = withProgrammers;
		WithAnalysts = withAnalysts;
	}
}