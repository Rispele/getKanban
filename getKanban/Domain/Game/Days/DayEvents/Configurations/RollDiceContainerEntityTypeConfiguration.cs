using System.Linq.Expressions;
using Domain.Game.Days.DayEvents.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.DayEvents.Configurations;

public class RollDiceContainerEntityTypeConfiguration : IEntityTypeConfiguration<RollDiceContainer>
{
	public void Configure(EntityTypeBuilder<RollDiceContainer> builder)
	{
		builder.HasKey(e => new { e.DayId, e.Id });

		builder.Property(e => e.Id).ValueGeneratedOnAdd();

		ConfigurePropertyConversion(builder, e => e.AnalystsDiceNumber);
		ConfigurePropertyConversion(builder, e => e.ProgrammersDiceNumber);
		ConfigurePropertyConversion(builder, e => e.TestersDiceNumber);
		ConfigurePropertyConversion(builder, e => e.AnalystsScores);
		ConfigurePropertyConversion(builder, e => e.AnalystsScores);
		ConfigurePropertyConversion(builder, e => e.TestersScores);
	}

	private static void ConfigurePropertyConversion<TProperty>(
		EntityTypeBuilder<RollDiceContainer> builder,
		Expression<Func<RollDiceContainer, TProperty>> propertyExpression)
	{
		builder
			.Property(propertyExpression)
			.IsRequired()
			.HasConversion(new ReadOnlyListConverter<int>());
	}
}