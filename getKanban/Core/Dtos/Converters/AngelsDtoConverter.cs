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

	public static AngelsDtoConverter For(ParticipantRole role) => new(role);

	public AngelsDto? Convert(ParticipantsContainer? participantsContainer)
	{
		return participantsContainer is null ? null : new AngelsDto
		{
			Id = InviteCodeHelper.ResolveTeamId(participantsContainer.InviteCode),
			Participants = Convert(participantsContainer.InviteCode, participantsContainer.Participants)
		};
	}

	private ParticipantsDto Convert(string? inviteCode, IReadOnlyList<Participant>? participants)
	{
		return new ParticipantsDto
		{
			InviteCode = inviteCode,
			Users = Convert(participants)
		};
	}

	private IReadOnlyList<UserDto>? Convert(IReadOnlyList<Participant>? participants)
	{
		return participants?.Select(
				x => new UserDto()
				{
					Id = x.User.Id,
					Name = x.User.Name
				})
			.ToList();
	}
}