using Domain.Game.Days.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class UpdateTeamRolesContainerEntityTypeConfiguration : IEntityTypeConfiguration<UpdateTeamRolesContainer>
{
	public void Configure(EntityTypeBuilder<UpdateTeamRolesContainer> builder)
	{
		builder.HasKey(e => e.Id);

		builder
			.HasMany<TeamRoleUpdate>("teamRoleUpdates")
			.WithOne();

		builder.Ignore(t => t.TeamRoleUpdates);

		builder.Navigation("teamRoleUpdates").AutoInclude();
	}
}