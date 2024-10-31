using Domain.Game.Configuration;
using Domain.Game.Teams;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game;

[EntityTypeConfiguration(typeof(GameSessionEntityTypeConfiguration))]
public class GameSession
{
	private readonly List<Participant> angels;

	private readonly List<Team> teams;

	public Guid Id { get; }

	public string Name { get; }

	public GameSession(
		User creator,
		string name,
		long teamsCount)
	{
		Id = Guid.NewGuid();
		Name = name;
		angels = [];
		teams = [];

		for (var i = 0; i < teamsCount; i++)
		{
			teams.Add(new Team(Id, $"Untitled #{i + 1}"));
		}

		angels.Add(new Participant(creator, ParticipantRole.Creator | ParticipantRole.Angel));
	}

	public bool HasAccess(Guid userId, Guid teamId)
	{
		return angels.SingleOrDefault(a => a.User.Id == userId)?.Role.Equals(ParticipantRole.Angel)
		       ?? teams.SingleOrDefault(t => t.Id == teamId)?.HasAccess(userId)
		       ?? throw new InvalidOperationException("Unknown team");
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