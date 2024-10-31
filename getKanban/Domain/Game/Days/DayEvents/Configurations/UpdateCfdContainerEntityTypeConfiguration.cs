using Domain.Game.Days.DayEvents.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.Configurations;

public class UpdateCfdContainerEntityTypeConfiguration : IEntityTypeConfiguration<UpdateCfdContainer>
{
	public void Configure(EntityTypeBuilder<UpdateCfdContainer> builder)
	{
		builder.HasKey(t => new { t.DayId, t.Id });
	}
}