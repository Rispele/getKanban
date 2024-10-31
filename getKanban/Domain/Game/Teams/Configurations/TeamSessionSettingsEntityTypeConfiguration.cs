using Domain.Game.Days.DayEvents.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Teams.Configurations;

public class TeamSessionSettingsEntityTypeConfiguration : IEntityTypeConfiguration<TeamSessionSettings>
{
	public void Configure(EntityTypeBuilder<TeamSessionSettings> builder)
	{
		builder.Property(e => e.InitiallyTakenTickets)
			.HasConversion(new ReadOnlyListConverter<string>());
	}
}