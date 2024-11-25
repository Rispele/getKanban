using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Configuration;

public class HubConnectionEntityTypeConfiguration : IEntityTypeConfiguration<HubConnection>
{
	public void Configure(EntityTypeBuilder<HubConnection> builder)
	{
		builder.HasKey(x => x.UserId);
	}
}