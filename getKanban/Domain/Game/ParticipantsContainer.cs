using Domain.Game.Configuration;
using Domain.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game;

[EntityTypeConfiguration(typeof(ParticipantsContainerEntityTypeConfiguration))]
public class ParticipantsContainer
{
	private readonly List<Participant> participants = null!;

	public long Id { get; }

	public string InviteCode { get; } = null!;
	public IReadOnlyList<Participant> Participants => participants;

	[UsedImplicitly]
	private ParticipantsContainer()
	{
	}

	public ParticipantsContainer(Guid parentId)
	{
		participants = [];
		InviteCode = $"{parentId}#{Guid.NewGuid()}";
	}
	
	public ParticipantsContainer(Guid parentId, Guid childId)
	{
		participants = [];
		InviteCode = $"{parentId}#{childId}";
	}

	internal (bool matched, bool updated) AddParticipantIfMatchInviteCode(
		string inviteCode,
		User user,
		ParticipantRole participantRole)
	{
		return InviteCode != inviteCode
			? (false, false)
			: (true, AddParticipant(user, participantRole));
	}

	public bool AddParticipant(User user, ParticipantRole participantRole)
	{
		if (participants.Any(t => t.User.Id == user.Id))
		{
			return false;
		}

		participants.Add(new Participant(user, participantRole));
		return true;
	}
}