using Domain.Game.Teams;
using Domain.Users;

namespace Domain.Game;

public class GameSession
{
	private readonly List<Participant> angels;

	private readonly List<Team> teams;

	public long Id { get; }

	public string Name { get; }

	public GameSession(
		User creator,
		string name,
		long teamsCount)
	{
		Name = name;
		angels = new List<Participant>();
		teams = new List<Team>();

		for (var i = 0; i < teamsCount; i++)
		{
			teams.Add(new Team($"Untitled #{i + 1}"));
		}

		angels.Add(new Participant(creator, ParticipantRole.Creator, ParticipantRole.Angel));
	}

	public void AddAngel(User user)
	{
		angels.Add(new Participant(user, ParticipantRole.Angel));
	}

	public void AddPlayer(User user, Guid teamId)
	{
		var team = teams.Single(t => t.Id == teamId);
		team.AddPlayer(user);
	}
}