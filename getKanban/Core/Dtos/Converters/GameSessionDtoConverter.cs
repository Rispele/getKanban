using Domain.Game;
using Domain.Game.Teams;
using Domain.Users;

namespace Core.Dtos.Converters;

public class GameSessionDtoConverter
{
	private readonly ParticipantRole requesterRole;

	private GameSessionDtoConverter(ParticipantRole requesterRole)
	{
		this.requesterRole = requesterRole;
	}

	public static GameSessionDtoConverter For(ParticipantRole role) => new(role);

	public GameSessionDto Convert(GameSession gameSession)
	{
		return new GameSessionDto
		{
			Id = gameSession.Id,
			Name = gameSession.Name,
			Angels = ConvertAngels(gameSession.Angels),
			Teams = gameSession.Teams.Select(Convert).ToArray(),
			RequesterRole = requesterRole,
			IsRecruitmentFinished = gameSession.IsRecruitmentFinished
		};
	}

	private TeamDto Convert(Team team)
	{
		return new TeamDto
		{
			Id = team.Id,
			Name = team.Name,
			Participants = Convert(team.Players),
			IsTeamSessionEnded = team.IsTeamSessionEnded
		};
	}

	private TeamDto ConvertAngels(ParticipantsContainer angels)
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
			participantsContainer.Participants.Select(p => Convert(p.User)).ToList());
	}

	private string? ConvertInviteCode(string inviteCode)
	{
		var isPermittedRole = (requesterRole & ParticipantRole.Angel) == ParticipantRole.Angel
		                   || (requesterRole & ParticipantRole.Creator) == ParticipantRole.Creator;
		return isPermittedRole ? inviteCode : null;
	}

	private UserDto Convert(User user)
	{
		return new UserDto
		{
			Id = user.Id,
			Name = user.Name
		};
	}
}