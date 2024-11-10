using Core.Helpers;
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

	public ParticipantRole EnsureHasAnyAccess(User user, string inviteCode)
	{
		return EnsureHasAccess(user) ?? 
		       EnsureHasInviteCodeAccess(inviteCode) ?? 
		       throw new InvalidOperationException($"User {user.Id} has no access to this team.");
	}

	public ParticipantRole? EnsureHasInviteCodeAccess(string inviteCode)
	{
		if (Angels.InviteCode == inviteCode)
		{
			return ParticipantRole.Angel;
		}
		
		var teamId = InviteCodeHelper.ResolveTeamId(inviteCode);
		var inviteTeamToJoin = Teams.SingleOrDefault(x => x.Id == teamId);
		if (inviteTeamToJoin is not null)
		{
			return ParticipantRole.Player;
		}

		return null;
	}

	public ParticipantRole? EnsureHasAccess(User user)
	{
		if (Angels.Contains(user))
		{
			return Angels.GetParticipant(user).Role;
		}

		var teamToJoin = teams.SingleOrDefault(x => x.Players.Contains(user));
		return teamToJoin?.Players.GetParticipant(user).Role;
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