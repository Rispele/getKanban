using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.DayContainers.Configurations;

public class UpdateTeamRolesContainerEntityTypeConfiguration : IEntityTypeConfiguration<UpdateTeamRolesContainer>
{
	public void Configure(EntityTypeBuilder<UpdateTeamRolesContainer> builder)
	{
		builder.Property(e => e.Timestamp).IsRowVersion();

		builder
			.HasMany<UpdateTeamRolesContainer>()
			.WithOne();
	}
}