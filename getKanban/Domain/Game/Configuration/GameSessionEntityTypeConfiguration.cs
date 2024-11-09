using Domain.Game.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Configuration;

public class GameSessionEntityTypeConfiguration : IEntityTypeConfiguration<GameSession>
{
	public void Configure(EntityTypeBuilder<GameSession> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id).ValueGeneratedOnAdd();

		builder.Property(x => x.Name).IsRequired();

		builder.Ignore(t => t.Teams);

		builder
			.HasMany<Team>("teams")
			.WithOne()
			.HasForeignKey(t => t.GameSessionId);

		builder
			.HasOne<ParticipantsContainer>(t => t.Angels)
			.WithMany();

		builder.Navigation(t => t.Angels).AutoInclude();
		builder.Navigation("teams").AutoInclude();
	}
}