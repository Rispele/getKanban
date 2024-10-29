namespace Domain.Game.Days.DayEvents.UpdateCfdDayEvent;

public class UpdateCfdDayEvent : DayEvent
{
	private UpdateCfdDayEvent(
		int dayId,
		int id,
		int released,
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
		: base(dayId, id, DayEventType.UpdateCfd)
	{
		Released = released;
		ReadyToDeploy = readyToDeploy;
		WithTesters = withTesters;
		WithProgrammers = withProgrammers;
		WithAnalysts = withAnalysts;
	}

	public int Released { get; }
	public int ReadyToDeploy { get; }
	public int WithTesters { get; }
	public int WithProgrammers { get; }
	public int WithAnalysts { get; }

	internal static void CreateInstance(
		DayContext dayContext,
		int released,
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		var @event = new UpdateCfdDayEvent(
			dayContext.DayId,
			dayContext.NextEventId,
			released,
			readyToDeploy,
			withTesters,
			withProgrammers,
			withAnalysts);
		dayContext.PostDayEvent(@event);
	}
}