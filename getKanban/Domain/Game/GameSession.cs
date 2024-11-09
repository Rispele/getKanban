using Domain.Game.Configuration;
using Domain.Game.Teams;
using Domain.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game;

[EntityTypeConfiguration(typeof(GameSessionEntityTypeConfiguration))]
public class GameSession
{
	public Guid Id { get; }
	public string Name { get; } = null!;

	public ParticipantsContainer Angels { get; } = null!;
	private List<Team> teams { get; } = null!;

	public IReadOnlyList<Team> Teams => teams;

	[UsedImplicitly]
	private GameSession()
	{
	}

	public GameSession(
		User creator,
		string name,
		long teamsCount)
	{
		Id = Guid.NewGuid();
		Name = name;
		Angels = new ParticipantsContainer(Id);
		teams = [];

		for (var i = 0; i < teamsCount; i++)
		{
			teams.Add(new Team(Id, $"Untitled #{i + 1}"));
		}

		Angels.AddParticipant(creator, ParticipantRole.Creator | ParticipantRole.Angel);
	}

	public ParticipantRole EnsureHasAccess(User user, Guid? sessionId = null, Guid? teamId = null)
	{
		var team = Teams.SingleOrDefault(x => x.Id == teamId);
		if (teamId != null && team != null)
		{
			AddByInviteCode(user, $"{sessionId}#{teamId}");
		}
		
		var angel = Angels.Participants.SingleOrDefault(a => a.User.Id == user.Id);
		if (angel is not null && (angel.Role & ParticipantRole.Angel) != 0)
		{
			return angel.Role;
		}
		
		if (teamId.HasValue)
		{
			if (teams.SingleOrDefault(t => t.Id == teamId.Value)?.HasAccess(user.Id) ?? false)
			{
				return ParticipantRole.Player;
			}
		}
		else
		{
			if (teams.Any(t => t.HasAccess(user.Id)))
			{
				return ParticipantRole.Player;
			}
		}

		throw new InvalidOperationException($"User {user.Id} has not access to this team.");
	}

	public (Guid teamId, bool updated) AddByInviteCode(User user, string inviteCode)
	{
		var angelsUpdateResult = Angels
			.AddParticipantIfMatchInviteCode(inviteCode, user, ParticipantRole.Angel);
		if (angelsUpdateResult.matched)
		{
			return (Guid.Empty, angelsUpdateResult.updated);
		}

		foreach (var team in teams)
		{
			var teamUpdateResult = team.AddByInviteCode(user, inviteCode);
			if (teamUpdateResult.matched)
			{
				return (team.Id, teamUpdateResult.updated);
			}
		}
		
		throw new InvalidOperationException("Unknown invite code");
	}

	public void Start()
	{
		teams.ForEach(t => t.StartSession());
	}
}