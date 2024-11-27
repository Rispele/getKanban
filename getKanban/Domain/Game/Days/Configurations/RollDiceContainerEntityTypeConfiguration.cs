using System.Linq.Expressions;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.RollDice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class RollDiceContainerEntityTypeConfiguration : IEntityTypeConfiguration<RollDiceContainer>
{
	public void Configure(EntityTypeBuilder<RollDiceContainer> builder)
	{
		builder.ConfigureAsDayContainer();

		ConfigurePropertyConversion(builder, e => e.DiceRollResults);
	}

	private static void ConfigurePropertyConversion<TProperty>(
		EntityTypeBuilder<RollDiceContainer> builder,
		Expression<Func<RollDiceContainer, TProperty>> propertyExpression)
	{
		builder
			.Property(propertyExpression)
			.IsRequired()
			.HasConversion(new ReadOnlyListConverter<DiceRollResult>());
	}
}