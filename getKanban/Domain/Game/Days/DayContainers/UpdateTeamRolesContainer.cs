using Domain.Game.Days.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers;

[EntityTypeConfiguration(typeof(UpdateTeamRolesContainerEntityTypeConfiguration))]
public class UpdateTeamRolesContainer
{
	private readonly List<TeamRoleUpdate> teamRoleUpdates = [];

	public long Id { get; }

	public IReadOnlyList<TeamRoleUpdate> TeamRoleUpdates => teamRoleUpdates;

	internal void AddUpdate(TeamRole from, TeamRole to)
	{
		teamRoleUpdates.Add(new TeamRoleUpdate { From = from, To = to });
	}

	internal void Remove(long updateId)
	{
		if (teamRoleUpdates.Any(t => t.Id == updateId))
		{
			teamRoleUpdates.Remove(teamRoleUpdates.Single(t => t.Id == updateId));
		}
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