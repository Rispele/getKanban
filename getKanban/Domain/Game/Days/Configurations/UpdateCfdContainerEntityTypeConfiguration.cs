using Domain.Game.Days.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class UpdateCfdContainerEntityTypeConfiguration : IEntityTypeConfiguration<UpdateCfdContainer>
{
	public void Configure(EntityTypeBuilder<UpdateCfdContainer> builder)
	{
		builder.ConfigureAsFreezableDayContainer();
		
		builder.Property(t => t.Released);
		builder.Property(t => t.ToDeploy);
		builder.Property(t => t.WithTesters);
		builder.Property(t => t.WithProgrammers);
		builder.Property(t => t.WithAnalysts);
		builder.Property(t => t.Frozen);
	}
}