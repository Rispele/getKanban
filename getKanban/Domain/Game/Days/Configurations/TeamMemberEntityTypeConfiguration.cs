using Domain.Game.Days.DayContainers.TeamMembers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class TeamMemberEntityTypeConfiguration : IEntityTypeConfiguration<TeamMember>
{
	public void Configure(EntityTypeBuilder<TeamMember> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(t => t.InitialRole).IsRequired();
		builder.Property(t => t.CurrentRole).IsRequired();

		builder.Property(t => t.Version).IsConcurrencyToken();
	}
}