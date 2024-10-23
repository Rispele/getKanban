namespace Domain.Game.Days.DayEvents.UpdateTeamRolesDayEvent;

public class UpdateTeamRolesDayEvent : DayEvent
{
	public UpdateTeamRolesDayEvent(TeamRole from, TeamRole to, int id)
		: base(DayEventType.UpdateTeamRoles, id)
	{
		From = from;
		To = to;
	}

	public TeamRole From { get; }
	public TeamRole To { get; }
}