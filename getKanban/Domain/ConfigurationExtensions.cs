using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;

public static class ConfigurationExtensions
{
	private const string PostgresRowVersionColumnName = "xmin";
	private const string PostgresRowVersionColumnType = "xid";

	public static PropertyBuilder<TProperty> ConfigureAsRowVersion<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
	{
		return propertyBuilder
			.HasColumnName(PostgresRowVersionColumnName)
			.HasColumnType(PostgresRowVersionColumnType)
			.IsRowVersion();
	}
}