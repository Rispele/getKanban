using Domain.Game.Days.DayEvents.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.Configurations;

public class WorkAnotherTeamContainerEntityTypeConfiguration : IEntityTypeConfiguration<WorkAnotherTeamContainer>
{
	public void Configure(EntityTypeBuilder<WorkAnotherTeamContainer> builder)
	{
		builder.HasKey(e => e.Id);
	}
}