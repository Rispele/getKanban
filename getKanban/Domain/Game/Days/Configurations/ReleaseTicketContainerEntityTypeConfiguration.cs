using Domain.Game.Days.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class ReleaseTicketContainerEntityTypeConfiguration : IEntityTypeConfiguration<ReleaseTicketContainer>
{
	public void Configure(EntityTypeBuilder<ReleaseTicketContainer> builder)
	{
		builder.Ignore(e => e.TicketIds);
		
		builder.ConfigureAsFreezableDayContainer();

		builder.Property(t => t.CanReleaseNotImmediatelyTickets);

		builder
			.Property("ticketIds")
			.IsRequired()
			.HasConversion(new ListConverter<string>());
	}
}