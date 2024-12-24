using Domain.DomainExceptions;
using Domain.Game.Days.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers.TeamMembers;

[EntityTypeConfiguration(typeof(TeamMembersContainerEntityTypeConfiguration))]
public class TeamMembersContainer : DayContainer
{
	private readonly List<TeamMember> teamMembers = null!;

	public IReadOnlyList<TeamMember> TeamMembers => teamMembers;
	public bool LockTesters { get; }

	[UsedImplicitly]
	private TeamMembersContainer()
	{
	}

	internal TeamMembersContainer(
		int analystsCount,
		int programmersCount,
		int testersCount,
		bool lockTesters)
	{
		LockTesters = lockTesters;
		teamMembers = [];

		Enumerable.Range(0, analystsCount).ForEach(_ => teamMembers.Add(new TeamMember(TeamRole.Analyst)));
		Enumerable.Range(0, programmersCount).ForEach(_ => teamMembers.Add(new TeamMember(TeamRole.Programmer)));
		Enumerable.Range(0, testersCount).ForEach(_ => teamMembers.Add(new TeamMember(TeamRole.Tester)));
	}

	internal void UpdateTeamMemberRole(long teamMemberId, TeamRole to)
	{
		if (to == TeamRole.Tester && LockTesters)
		{
			throw new DomainException("Could not change role, because testers are locked");
		}

		var teamMember = teamMembers.Single(t => t.Id == teamMemberId);

		if (teamMember.InitialRole == TeamRole.Tester && LockTesters)
		{
			throw new DomainException("Could not change role, because testers are locked");
		}

		teamMember.UpdateTeamRole(to);
		SetUpdated();
	}
}