using Domain.Game.Days.DayContainers;

namespace Core.Dtos;

public class UpdateTeamRolesContainerDto : DayContainerDto
{
	public IReadOnlyList<(long id, TeamRole from, TeamRole to)> TeamRoleUpdates { get; init; } = null!;
}