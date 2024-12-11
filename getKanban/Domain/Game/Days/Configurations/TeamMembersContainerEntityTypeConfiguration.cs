using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.TeamMembers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class TeamMembersContainerEntityTypeConfiguration : IEntityTypeConfiguration<TeamMembersContainer>
{
	public void Configure(EntityTypeBuilder<TeamMembersContainer> builder)
	{
		builder.ConfigureAsDayContainer();
		
		builder
			.HasMany<TeamMember>("teamMembers")
			.WithOne();

		builder.Property(t => t.LockTesters);

		builder.Ignore(t => t.TeamMembers);

		builder.Navigation("teamMembers").AutoInclude();
	}
}