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
		return context.GameSessions.SingleOrDefaultAsync(g => g.Id == gameSessionId);
	}

	public static async Task<Team> GetTeamAsync(this DomainContext teamsContext, Guid gameSessionId, Guid teamId)
	{
		var team = await teamsContext.Teams.SingleOrDefaultAsync(
			t => t.GameSessionId == gameSessionId && t.Id == teamId);
		return team ?? throw new InvalidOperationException($"Team with id {teamId} not found");
	}

	public static Task<Team?> FindTeamAsync(this DomainContext teamsContext, Guid gameSessionId, Guid teamId)
	{
		return teamsContext.Teams.SingleOrDefaultAsync(t => t.GameSessionId == gameSessionId && t.Id == teamId);
	}

	public static async Task<User> GetUserAsync(this DomainContext usersContext, Guid userId)
	{
		var user = await usersContext.Users.SingleOrDefaultAsync(t => t.Id == userId);
		return user ?? throw new InvalidOperationException("User not found");
	}
	
	public static async Task CloseRecruitmentAsync(this DomainContext gameSessionContext, Guid sessionId)
	{
		var session = await gameSessionContext.GameSessions.SingleOrDefaultAsync(x => x.Id == sessionId);
		if (session is { IsRecruitmentFinished: false })
		{
			session.IsRecruitmentFinished = true;
		}
	}
}