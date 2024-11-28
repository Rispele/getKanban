using Domain.Game;
using Domain.Game.Teams;
using Domain.Users;

namespace Core.Dtos.Converters;

public class TeamDtoConverter
{
	private readonly ParticipantRole requesterRole;

	private TeamDtoConverter(ParticipantRole requesterRole)
	{
		this.requesterRole = requesterRole;
	}

	public static TeamDtoConverter For(ParticipantRole role) => new(role);

	public TeamDto Convert(Team team)
	{
		return new TeamDto
			{
				Id = team.Id,
				Name = team.Name,
				Participants = Convert(team.Players)
			};
	}

	public TeamDto ConvertAngels(ParticipantsContainer angels)
	{
		return new TeamDto
			{
				Id = angels.PublicId,
				Name = "Ангелы",
				Participants = Convert(angels)
			};
	}

	private ParticipantsDto Convert(ParticipantsContainer participantsContainer)
	{
		return new ParticipantsDto(
			ConvertInviteCode(participantsContainer.InviteCode),
			participantsContainer.Participants.Select(p => UserDtoConverter.Convert(p.User)).ToList()
		);
	}

	private string? ConvertInviteCode(string inviteCode)
	{
		var isPermittedRole = (requesterRole & ParticipantRole.Angel) == ParticipantRole.Angel
		                   || (requesterRole & ParticipantRole.Creator) == ParticipantRole.Creator;
		return isPermittedRole ? inviteCode : null;
	}
}