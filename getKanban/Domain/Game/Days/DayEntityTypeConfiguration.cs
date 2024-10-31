using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

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

		builder.Property("dayContext")
			.HasColumnName("day_context")
			.HasConversion(new DayContextConversion());

		builder.Property("analystsNumber");
		builder.Property("programmersNumber");
		builder.Property("testersNumber");

		builder.Property(e => e.Timestamp).IsRowVersion();
	}

	private class DayContextConversion : ValueConverter<DayContext, string>
	{
		public DayContextConversion()
			: base(
				context => JsonConvert.SerializeObject(context),
				str => JsonConvert.DeserializeObject<DayContext>(str)!)
		{
		}
	}
}