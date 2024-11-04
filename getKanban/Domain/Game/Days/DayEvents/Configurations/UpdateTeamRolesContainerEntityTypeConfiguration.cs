using Domain.Game.Days.DayEvents.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.Configurations;

public class UpdateTeamRolesContainerEntityTypeConfiguration : IEntityTypeConfiguration<UpdateTeamRolesContainer>
{
	public void Configure(EntityTypeBuilder<UpdateTeamRolesContainer> builder)
	{
		builder.HasKey(e => e.Id);

		builder
			.HasMany<TeamRoleUpdate>("teamRoleUpdates")
			.WithOne();

		builder.Navigation("teamRoleUpdates").AutoInclude();
	}
}