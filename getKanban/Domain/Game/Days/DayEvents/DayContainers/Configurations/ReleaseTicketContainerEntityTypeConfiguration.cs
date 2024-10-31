using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.DayContainers.Configurations;

public class ReleaseTicketContainerEntityTypeConfiguration : IEntityTypeConfiguration<ReleaseTicketContainer>
{
	public void Configure(EntityTypeBuilder<ReleaseTicketContainer> builder)
	{
		builder
			.Property(e => e.TicketIds)
			.IsRequired()
			.HasConversion(new ReadOnlyListConverter<string>());
	}
}