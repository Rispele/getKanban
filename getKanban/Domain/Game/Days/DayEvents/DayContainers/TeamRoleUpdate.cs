using Domain.Game.Days.DayEvents.DayContainers.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(TeamRoleUpdateEntityTypeConfiguration))]
public class TeamRoleUpdate
{
	public long Id { get; }
	public TeamRole From { get; init; }
	public TeamRole To { get; init; }
}