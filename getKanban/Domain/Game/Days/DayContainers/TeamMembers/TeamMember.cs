using Domain.Game.Days.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers.TeamMembers;

[EntityTypeConfiguration(typeof(TeamMemberEntityTypeConfiguration))]
public class TeamMember
{
	public long Id { get; }

	public TeamRole InitialRole { get; init; }
	public TeamRole CurrentRole { get; private set; }
	
	public long Version { get; [UsedImplicitly] private set; }

	[UsedImplicitly]
	private TeamMember()
	{
	}

	public TeamMember(TeamRole initialRole)
	{
		InitialRole = initialRole;
		CurrentRole = initialRole;
	}

	internal void UpdateTeamRole(TeamRole newRole)
	{
		CurrentRole = newRole;
	}
}