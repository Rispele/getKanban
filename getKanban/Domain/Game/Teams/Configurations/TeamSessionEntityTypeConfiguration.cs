using Domain.Game.Days;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Teams.Configurations;

public class TeamSessionEntityTypeConfiguration : IEntityTypeConfiguration<TeamSession>
{
	public void Configure(EntityTypeBuilder<TeamSession> builder)
	{
		builder.HasKey(e => new { e.TeamId, e.Id });
		
		builder.Property(e => e.Id).ValueGeneratedOnAdd();

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
			.WithOne();
	}
}