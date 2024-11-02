﻿using Domain.Game.Days.DayEvents.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.Configurations;

public class TeamRoleUpdateEntityTypeConfiguration : IEntityTypeConfiguration<TeamRoleUpdate>
{
	public void Configure(EntityTypeBuilder<TeamRoleUpdate> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id).ValueGeneratedOnAdd();
	}
}