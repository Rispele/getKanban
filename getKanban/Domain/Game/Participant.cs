using Domain.Users;

namespace Domain.Game;

public class Participant
{
	public ParticipantRole Role { get; }
	public User User { get; }

	public Participant(User user, ParticipantRole role)
	{
		User = user;
		Role = role;
	}
}