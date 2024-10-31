using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.DayContainers.Configurations;

public class UpdateSprintBacklogContainerEntityTypeConfiguration : IEntityTypeConfiguration<UpdateSprintBacklogContainer>
{
	public void Configure(EntityTypeBuilder<UpdateSprintBacklogContainer> builder)
	{
		builder.Property(e => e.TicketIds)
			.HasConversion(new ReadOnlyListConverter<string>());
	}
}