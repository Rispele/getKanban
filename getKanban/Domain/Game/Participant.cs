using Domain.Users;

namespace Domain.Game;

public class Participant
{
	private readonly User user;

	private readonly List<Role> roles;

	public Participant(User user, params Role[] roles)
	{
		this.user = user;
		this.roles = new List<Role>(roles);
	}
}