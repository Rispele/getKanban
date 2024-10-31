namespace Domain.Game.Days.DayEvents.DayContainers;

public class UpdateTeamRolesContainer
{
	private readonly List<TeamRoleUpdate> teamRoleUpdates = null!;

	public int DayId { get; }

	private UpdateTeamRolesContainer()
	{
	}

	public UpdateTeamRolesContainer(int dayId)
	{
		DayId = dayId;
		teamRoleUpdates = [];
	}

	public void AddUpdate(Day day, TeamRole from, TeamRole to)
	{
		day.PostDayEvent(DayEventType.UpdateTeamRoles);

		teamRoleUpdates.Add(new TeamRoleUpdate { From = from, To = to });
	}

	public Dictionary<TeamRole, TeamRole[]> BuildTeamRolesUpdate()
	{
		return teamRoleUpdates
			.GroupBy(@event => @event.From)
			.ToDictionary(
				grouping => grouping.Key,
				grouping => grouping.Select(@event => @event.To).ToArray());
	}
}