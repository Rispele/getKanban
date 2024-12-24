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
	public bool IsRecruitmentFinished = false;
	private List<Team> teams { get; } = null!;
	public Guid Id { get; }
	public string Name { get; } = null!;

	public ParticipantsContainer Angels { get; } = null!;

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

	public Team? FindUserTeam(Guid userId)
	{
		return Teams.SingleOrDefault(x => x.Players.Contains(userId));
	}

	public ParticipantRole EnsureHasAnyAccess(Guid userId, string inviteCode)
	{
		return EnsureHasAccess(userId) ??
		       EnsureHasInviteCodeAccess(inviteCode) ??
		       throw new InvalidOperationException($"User {userId} has no access to this team.");
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

	public ParticipantRole? EnsureHasAccess(Guid userId)
	{
		return Angels.Contains(userId)
			? Angels.GetParticipant(userId).Role
			: FindUserTeam(userId)?.Players.GetParticipant(userId).Role;
	}

	public (Guid teamId, bool updated) AddByInviteCode(User user, string inviteCode)
	{
		var angelsUpdateResult = Angels
			.AddParticipantIfMatchInviteCode(inviteCode, user, ParticipantRole.Angel);
		if (angelsUpdateResult.matched)
		{
			return (Angels.PublicId, angelsUpdateResult.updated);
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