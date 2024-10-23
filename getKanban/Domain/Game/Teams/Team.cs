using Domain.Users;

namespace Domain.Game.Teams;

public class Team
{
	private readonly List<Participant> participants;

	private readonly TeamSession teamSession;

	public Team(string name)
	{
		Id = Guid.NewGuid();
		Name = name;
		teamSession = new TeamSession();
		participants = new List<Participant>();
	}

	public Guid Id { get; }

	public string Name { get; private set; }

	public void AddPlayer(User user)
	{
		participants.Add(new Participant(user, ParticipantRole.Player));
	}
}