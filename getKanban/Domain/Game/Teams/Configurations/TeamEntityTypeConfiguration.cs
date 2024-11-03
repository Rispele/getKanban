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

		builder.Ignore("currentDay");
		builder.Ignore("previousDay");

		builder.Ignore(e => e.TakenTickets);
		builder.Ignore(e => e.ReleasedTickets);
		builder.Ignore(e => e.TicketsInWork);
		builder.Ignore(e => e.AnotherTeamScores);

		builder
			.HasMany<Day>()
			.WithOne();

		builder
			.HasOne<TeamSessionSettings>()
			.WithOne()
			.HasForeignKey<TeamSessionSettings>();

		builder
			.HasMany<Participant>()
			.WithMany();
	}
}