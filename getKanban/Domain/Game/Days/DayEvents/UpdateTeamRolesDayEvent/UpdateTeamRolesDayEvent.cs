namespace Domain.Game.Days.DayEvents.UpdateTeamRolesDayEvent;

public class UpdateTeamRolesDayEvent : DayEvent
{
	public TeamRole From { get; }
	public TeamRole To { get; }

	public UpdateTeamRolesDayEvent(TeamRole from, TeamRole to, int id)
		: base(DayEventType.UpdateTeamRoles, id)
	{
		From = from;
		To = to;
	}
}