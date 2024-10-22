using Domain.Game.Days;
using Domain.Users;

namespace Domain.Game.Teams;

public class Team
{
	public Guid Id { get; }

	public string Name { get; private set; }

	private readonly List<Participant> participants;

	private readonly DayContainer dayContainer;

	public Team(string name)
	{
		Id = Guid.NewGuid();
		Name = name;
		participants = new List<Participant>();
		dayContainer = new DayContainer();
	}

	public void AddPlayer(User user)
	{
		participants.Add(new Participant(user, Role.Player));
	}
}