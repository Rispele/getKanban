using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Configuration;

public class ParticipantEntityTypeConfiguration : IEntityTypeConfiguration<Participant>
{
	public void Configure(EntityTypeBuilder<Participant> builder)
	{
		builder.HasKey(t => t.Id);

		builder.Property(t => t.Role);

		builder
			.HasOne<User>()
			.WithMany();
	}
}