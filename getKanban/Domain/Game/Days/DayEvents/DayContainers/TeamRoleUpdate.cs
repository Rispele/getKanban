namespace Domain.Game.Days.DayEvents.DayContainers;

public class TeamRoleUpdate
{
	public long Id { get; }
	public TeamRole From { get; init; }
	public TeamRole To { get; init; }
}