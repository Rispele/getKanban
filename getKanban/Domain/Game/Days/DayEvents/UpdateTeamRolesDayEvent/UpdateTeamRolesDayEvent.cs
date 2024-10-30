namespace Domain.Game.Days.DayEvents.UpdateTeamRolesDayEvent;

public class UpdateTeamRolesDayEvent : DayEvent
{
	private UpdateTeamRolesDayEvent(
		int dayId,
		int id,
		TeamRole from,
		TeamRole to)
		: base(dayId, id, DayEventType.UpdateTeamRoles)
	{
		From = from;
		To = to;
	}

	public TeamRole From { get; }
	public TeamRole To { get; }

	internal static void CreateInstance(DayContext dayContext, TeamRole from, TeamRole to)
	{
		var @event = new UpdateTeamRolesDayEvent(
			dayContext.DayId,
			dayContext.NextEventId,
			from,
			to);
		dayContext.PostDayEvent(@event);
	}
}