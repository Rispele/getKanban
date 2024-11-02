using Domain.Game.Days.DayEvents.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.Configurations;

public class
	UpdateSprintBacklogContainerEntityTypeConfiguration : IEntityTypeConfiguration<UpdateSprintBacklogContainer>
{
	public void Configure(EntityTypeBuilder<UpdateSprintBacklogContainer> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.TicketIds)
			.HasConversion(new ReadOnlyListConverter<string>());
	}
}