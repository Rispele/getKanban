namespace Core.Dtos.Containers.TeamMembers;

public class TeamMembersContainerDto : DayContainerDto
{
	public IReadOnlyList<TeamMemberDto> TeamRoleUpdates { get; init; } = null!;
	
	public bool LockTesters { get; init; }
}