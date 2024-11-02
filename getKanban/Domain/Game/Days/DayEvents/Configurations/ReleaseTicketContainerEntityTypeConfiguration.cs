using Domain.Game.Days.DayEvents.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.Configurations;

public class ReleaseTicketContainerEntityTypeConfiguration : IEntityTypeConfiguration<ReleaseTicketContainer>
{
	public void Configure(EntityTypeBuilder<ReleaseTicketContainer> builder)
	{
		builder.HasKey(e => e.Id );

		builder
			.Property(e => e.TicketIds)
			.IsRequired()
			.HasConversion(new ReadOnlyListConverter<string>());
	}
}