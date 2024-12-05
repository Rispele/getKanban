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
}