using Domain.Game.Days;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Teams.Configurations;

public class TeamEntityTypeConfiguration : IEntityTypeConfiguration<Team>
{
	public void Configure(EntityTypeBuilder<Team> builder)
	{
		builder.HasKey(x => new { x.GameSessionId, x.Id });

		builder.Property(x => x.GameSessionId).IsRequired();

		builder.Property("currentDayNumber");

		builder.Ignore("previousDay");
		
		builder.Ignore(d => d.CurrentDay);
		builder.Ignore(d => d.Days);

		builder.Ignore(e => e.CurrentDayTeamRoleUpdates);
		builder.Ignore(e => e.CfdContainers);

		builder.Property(e => e.RowVersions).ConfigureAsRowVersion();

		builder
			.HasMany<Day>("days")
			.WithOne();

		builder.Ignore("settings");
		
		// builder
		// 	.HasOne<TeamSessionSettings>("settings")
		// 	.WithOne()
		// 	.HasForeignKey<TeamSessionSettings>();

		builder
			.HasOne<ParticipantsContainer>(t => t.Players)
			.WithMany();

		builder.Navigation("days").AutoInclude();
		builder.Navigation("settings").AutoInclude();
		builder.Navigation(t => t.Players).AutoInclude();
	}
}