using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.Configurations;

public class AwaitedEventEntityTypeConfiguration : IEntityTypeConfiguration<AwaitedEvent>
{
	public void Configure(EntityTypeBuilder<AwaitedEvent> builder)
	{
		builder.HasKey(x => x.Id);
	}
}