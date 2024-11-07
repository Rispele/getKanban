using System.Linq.Expressions;
using Domain.Game.Days.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class RollDiceContainerEntityTypeConfiguration : IEntityTypeConfiguration<RollDiceContainer>
{
	public void Configure(EntityTypeBuilder<RollDiceContainer> builder)
	{
		builder.ConfigureAsDayContainer();

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