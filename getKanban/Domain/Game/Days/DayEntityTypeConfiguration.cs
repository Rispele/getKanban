using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days;

public class DayEntityTypeConfiguration : IEntityTypeConfiguration<Day>
{
	public void Configure(EntityTypeBuilder<Day> builder)
	{
		builder.HasKey(e => new { e.TeamSessionId, DayId = e.Id });

		builder.Property(e => e.TeamSessionId)
			.HasColumnName("team_session_id");

		builder.Property(e => e.Id)
			.HasColumnName("id");

		builder.Property("analystsNumber");
		builder.Property("programmersNumber");
		builder.Property("testersNumber");
		
		builder.Property(e => e.Timestamp).IsRowVersion();
	}
}