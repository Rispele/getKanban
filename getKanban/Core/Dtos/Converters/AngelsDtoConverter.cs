using Core.Helpers;
using Domain.Game;

namespace Core.Dtos.Converters;

public class AngelsDtoConverter
{
	private readonly ParticipantRole requesterRole;

	private AngelsDtoConverter(ParticipantRole requesterRole)
	{
		this.requesterRole = requesterRole;
	}

	public static AngelsDtoConverter For(ParticipantRole role)
	{
		return new AngelsDtoConverter(role);
	}

	public AngelsDto Convert(ParticipantsContainer participantsContainer)
	{
		return new AngelsDto
		{
			Id = InviteCodeHelper.ResolveTeamId(participantsContainer.InviteCode),
			Participants = Convert(participantsContainer.InviteCode, participantsContainer.Participants)
		};
	}

	private ParticipantsDto Convert(string inviteCode, IReadOnlyList<Participant> participants)
	{
		return new ParticipantsDto(ConvertInviteCode(inviteCode), Convert(participants));
	}

	private string? ConvertInviteCode(string inviteCode)
	{
		var isPermittedRole = (requesterRole & ParticipantRole.Angel) == ParticipantRole.Angel
		                   || (requesterRole & ParticipantRole.Creator) == ParticipantRole.Creator;
		return isPermittedRole ? inviteCode : null;
	}

	private IReadOnlyList<UserDto> Convert(IReadOnlyList<Participant> participants)
	{
		return participants.Select(
				x => new UserDto
				{
					Id = x.User.Id,
					Name = x.User.Name
				})
			.ToList();
	}
}