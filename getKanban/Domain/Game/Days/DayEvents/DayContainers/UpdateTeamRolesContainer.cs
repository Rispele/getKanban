﻿using Domain.Game.Days.DayEvents.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(UpdateTeamRolesContainerEntityTypeConfiguration))]
public class UpdateTeamRolesContainer
{
	private readonly List<TeamRoleUpdate> teamRoleUpdates = null!;

	public long Id { get; }

	public byte[]? Timestamp { get; set; }

	[UsedImplicitly]
	public UpdateTeamRolesContainer()
	{
		teamRoleUpdates = [];
	}

	public void AddUpdate(Day day, TeamRole from, TeamRole to)
	{
		day.PostDayEvent(DayEventType.UpdateTeamRoles);

		teamRoleUpdates.Add(new TeamRoleUpdate { From = from, To = to });
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