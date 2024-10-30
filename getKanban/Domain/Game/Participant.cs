using Domain.Users;

namespace Domain.Game;

public class Participant
{
	public Participant(User user, ParticipantRole role)
	{
		User = user;
		Role = role;
	}

	public ParticipantRole Role { get; }
	public User User { get; }
}