using Domain.DbContexts;
using Domain.Game;
using Domain.Game.Teams;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Core.DbContexts.Extensions;

public static class DomainContextExtensions
{
	public static async Task<GameSession> GetGameSessionsAsync(this DomainContext context, Guid gameSessionId)
	{
		return await FindGameSessionsAsync(context, gameSessionId) 
		       ?? throw new InvalidOperationException($"Game session with id {gameSessionId} not found");
	}
	
	public static Task<GameSession?> FindGameSessionsAsync(this DomainContext context, Guid gameSessionId)
	{
		return context.GameSessions
			.Where(g => g.Id == gameSessionId)
			.Take(1)
			.FirstOrDefaultAsync();
	}

	public static async Task<Team> GetTeamAsync(this DomainContext teamsContext, Guid gameSessionId, Guid teamId)
	{
		var team = await teamsContext.FindTeamAsync(gameSessionId, teamId);
		return team ?? throw new InvalidOperationException($"Team with id {teamId} not found");
	}

	public static Task<Team?> FindTeamAsync(this DomainContext teamsContext, Guid gameSessionId, Guid teamId)
	{
		return teamsContext.Teams
			.Where(t => t.GameSessionId == gameSessionId && t.Id == teamId)
			.Take(1)
			.FirstOrDefaultAsync();
	}

	public static async Task<User> GetUserAsync(this DomainContext usersContext, Guid userId)
	{
		var user = await usersContext.Users.Where(t => t.Id == userId).FirstOrDefaultAsync();
		return user ?? throw new InvalidOperationException("User not found");
	}
	
	public static async Task CloseRecruitmentAsync(this DomainContext gameSessionContext, Guid sessionId)
	{
		var session = await gameSessionContext.GetGameSessionsAsync(sessionId);
		if (session is { IsRecruitmentFinished: false })
		{
			session.IsRecruitmentFinished = true;
		}
	}

	public static async Task<List<GameSession>> GetCurrentSession(this DomainContext gameSessionContext, Guid userId)
	{
		var teams = new List<Team>();
		foreach (var session in gameSessionContext.GameSessions)
		{
			foreach (var team in session.Teams)
			{
				teams.Add(team);
			}
		}
		
		var sessionTeams = teams.Where(
			t => t.Players.Participants.SingleOrDefault(p => p.User.Id == userId) != default).ToList();
		
		var angels = new List<ParticipantsContainer>();
		foreach (var session in gameSessionContext.GameSessions)
		{
			angels.Add(session.Angels);
		}
		
		var sessionAngels = angels.Where(
			t => t.Participants.SingleOrDefault(p => p.User.Id == userId) != default).ToList();

		var result = new List<GameSession>();
		
		if (sessionTeams.Count > 0)
		{
			foreach (var sessionTeam in sessionTeams)
			{
				foreach (var sess in gameSessionContext.GameSessions)
				{
					foreach (var t in sess.Teams)
					{
						if (t.Id == sessionTeam.Id)
						{
							result.Add(sess);
							break;
						}
					}
				}
			}
		}

		if (sessionAngels.Count > 0)
		{
			foreach (var angelsTeam in sessionAngels)
			{
				result.AddRange(gameSessionContext.GameSessions.Where(x => x.Angels.PublicId == angelsTeam.PublicId));
			}
		}

		return result;
	}
}