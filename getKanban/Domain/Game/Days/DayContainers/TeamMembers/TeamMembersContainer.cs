using Domain.Game.Days.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers.TeamMembers;

[EntityTypeConfiguration(typeof(TeamMembersContainerEntityTypeConfiguration))]
public class TeamMembersContainer : DayContainer
{
	private readonly List<TeamMember> teamMembers = null!;
	
	public IReadOnlyList<TeamMember> TeamMembers => teamMembers;

	[UsedImplicitly]
	private TeamMembersContainer()
	{
	}
	
	internal TeamMembersContainer(int analystsCount, int programmersCount, int testersCount)
	{
		teamMembers = [];
		
		Enumerable.Range(0, analystsCount).ForEach(_ => teamMembers.Add(new TeamMember(TeamRole.Analyst)));
		Enumerable.Range(0, programmersCount).ForEach(_ => teamMembers.Add(new TeamMember(TeamRole.Programmer)));
		Enumerable.Range(0, testersCount).ForEach(_ => teamMembers.Add(new TeamMember(TeamRole.Tester)));
	}
	
	internal void UpdateTeamMemberRole(long teamMemberId, TeamRole to)
	{
		teamMembers.Single(t => t.Id == teamMemberId).UpdateTeamRole(to);
		SetUpdated();
	}
}