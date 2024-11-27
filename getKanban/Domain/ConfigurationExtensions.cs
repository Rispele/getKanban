using Domain.Game.Days.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain;

public static class ConfigurationExtensions
{
	private const string PostgresRowVersionColumnName = "xmin";
	private const string PostgresRowVersionColumnType = "xid";

	public static void ConfigureAsFreezableDayContainer<T>(this EntityTypeBuilder<T> builder)
		where T : FreezableDayContainer
	{
		builder.Property(e => e.Frozen);
		
		builder.ConfigureAsDayContainer();
	}

	
	public static void ConfigureAsDayContainer<T>(this EntityTypeBuilder<T> builder)
		where T : DayContainer
	{
		builder.HasKey(dc => dc.Id);
		
		builder.Property(e => e.Version).IsConcurrencyToken();
		builder.Property(t => t.Timestamp).ConfigureAsRowVersion();

		builder.Ignore(t => t.IsUpdated);
	}

	public static PropertyBuilder<TProperty> ConfigureAsRowVersion<TProperty>(
		this PropertyBuilder<TProperty> propertyBuilder)
	{
		return propertyBuilder
			.HasColumnName(PostgresRowVersionColumnName)
			.HasColumnType(PostgresRowVersionColumnType)
			.IsRowVersion();
	}
}