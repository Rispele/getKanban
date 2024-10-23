using Domain.Users;

namespace Domain.Game;

public class Participant
{
	private readonly List<ParticipantRole> roles;
	private readonly User user;

	public Participant(User user, params ParticipantRole[] roles)
	{
		this.user = user;
		this.roles = new List<ParticipantRole>(roles);
	}
}