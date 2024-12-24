using Domain.Game.Days.DayContainers.TeamMembers;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class UpdateTeamRolesCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.UpdateTeamRoles;

	public long TeamMemberId { get; init; }
	public TeamRole To { get; init; }

	internal override void Execute(Team _, Day day)
	{
		day.EnsureCanPostEvent(CommandType);

		day.TeamMembersContainer.UpdateTeamMemberRole(TeamMemberId, To);

		day.PostDayEvent(CommandType, null);
	}
}