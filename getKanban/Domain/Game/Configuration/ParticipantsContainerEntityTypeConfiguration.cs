using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Game.Configuration;

public class ParticipantsContainerEntityTypeConfiguration : IEntityTypeConfiguration<ParticipantsContainer>
{
	public void Configure(EntityTypeBuilder<ParticipantsContainer> builder)
	{
		builder.HasKey(pc => pc.Id);
		
		builder.Property(pc => pc.InviteCode).IsRequired();
		builder.Ignore(pc => pc.Participants);
		
		builder.HasMany("participants").WithOne();
		builder.Navigation("participants").AutoInclude();
	}
}