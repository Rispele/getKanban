using Core.Helpers;
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

	public ParticipantsContainer(Guid parentId, Guid? childId = null)
	{
		participants = [];
		var providedChildId = childId ?? Guid.NewGuid(); 
		InviteCode = InviteCodeHelper.ConcatInviteCode(parentId, providedChildId);
	}

	internal (bool matched, bool updated) AddParticipantIfMatchInviteCode(
		string inviteCode,
		User user,
		ParticipantRole participantRole)
	{
		return MatchInviteCode(inviteCode)
			? (true, AddParticipant(user, participantRole))
			: (false, false);
	}

	public Participant GetParticipant(User user)
	{
		return participants.Single(p => p.User.Id == user.Id);
	}

	public bool Contains(User user)
	{
		return participants.Any(p => p.User.Id == user.Id);
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
	
	public bool MatchInviteCode(string inviteCode)
	{
		return InviteCode == inviteCode;
	}
}