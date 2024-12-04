﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days;

public class DaySettingsEntityTypeConfiguration : IEntityTypeConfiguration<DaySettings>
{
	public void Configure(EntityTypeBuilder<DaySettings> builder)
	{
		builder.Property(t => t.Number).IsRequired();
		
		builder.Property(t => t.AnalystsCount).IsRequired();
		builder.Property(t => t.ProgrammersCount).IsRequired();
		builder.Property(t => t.TestersCount).IsRequired();

		builder.Property(t => t.ProfitPerClient).IsRequired();
	}
}