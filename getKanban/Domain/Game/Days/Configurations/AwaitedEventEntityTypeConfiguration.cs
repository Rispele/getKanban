using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Days.Configurations;

public class AwaitedEventEntityTypeConfiguration : IEntityTypeConfiguration<AwaitedCommands>
{
	public void Configure(EntityTypeBuilder<AwaitedCommands> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(t => t.CommandType);
	}
}