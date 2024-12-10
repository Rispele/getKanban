using Domain.Game.Days.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class
	UpdateSprintBacklogContainerEntityTypeConfiguration : IEntityTypeConfiguration<UpdateSprintBacklogContainer>
{
	public void Configure(EntityTypeBuilder<UpdateSprintBacklogContainer> builder)
	{
		builder.ConfigureAsFreezableDayContainer();
		builder.Property("ticketIds").HasColumnType("json").HasConversion(new ListConverter<string>());
		
		builder.Ignore("descriptors");
		builder.Ignore(e => e.TicketIds);
	}
}