using Domain.Game.Days.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers;

[EntityTypeConfiguration(typeof(TeamRoleUpdateEntityTypeConfiguration))]
public class TeamRoleUpdate
{
	public long Id { get; }
	public TeamRole From { get; init; }
	public TeamRole To { get; init; }
}