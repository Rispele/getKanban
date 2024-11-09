using Domain.Game;
using Domain.Game.Teams;
using Domain.Users;

namespace Core.Dtos;

public class GameSessionConverter
{
	public GameSessionDto Convert(GameSession gameSession)
	{
		return new GameSessionDto
		{
			Id = gameSession.Id,
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
			InviteCode = participantsContainer.InviteCode,
			Users = participantsContainer.Participants.Select(p => Convert(p.User)).ToList()
		};
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