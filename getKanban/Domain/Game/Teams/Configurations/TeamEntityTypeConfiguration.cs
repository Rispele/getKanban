using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Teams.Configurations;

public class TeamEntityTypeConfiguration : IEntityTypeConfiguration<Team>
{
	public void Configure(EntityTypeBuilder<Team> builder)
	{
		builder.HasKey(x => new { x.GameSessionId, x.Id });

		builder.Property(x => x.GameSessionId).IsRequired();

		builder
			.HasOne<TeamSession>()
			.WithOne()
			.HasPrincipalKey<TeamSession>(s => s.TeamId);

		builder
			.HasMany<Participant>()
			.WithMany();
	}
}