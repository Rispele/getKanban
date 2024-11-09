using Domain.Game;
using Domain.Game.Teams;
using Domain.Users;

namespace Core.Dtos.Builders;

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
			Angels = Convert(gameSession.Angels),
			Teams = gameSession.Teams.Select(Convert).ToArray()
		};
	}

	private TeamDto Convert(Team team)
	{
		return new TeamDto
		{
			Id = team.Id,
			Name = team.Name,
			Participants = Convert(team.Players)
		};
	}
	
	private ParticipantsDto Convert(ParticipantsContainer participantsContainer)
	{
		return new ParticipantsDto
		{
			InviteCode = ConvertInviteCode(participantsContainer.InviteCode),
			Users = participantsContainer.Participants.Select(p => Convert(p.User)).ToList()
		};
	}

	private string? ConvertInviteCode(string inviteCode)
	{
		return (requesterRole & ParticipantRole.Angel) != 0 ? null : inviteCode;
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