﻿using Domain.Game.Days.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class WorkAnotherTeamContainerEntityTypeConfiguration : IEntityTypeConfiguration<WorkAnotherTeamContainer>
{
	public void Configure(EntityTypeBuilder<WorkAnotherTeamContainer> builder)
	{
		builder.ConfigureAsDayContainer();

		builder.Property(x => x.DiceNumber);
		builder.Property(x => x.ScoresNumber);
	}
}