using Domain.Game.Teams;
using Microsoft.EntityFrameworkCore;

namespace Core.DbContexts.Extensions;

public static class TeamsContextExtensions
{
	public static async Task<Team> GetTeam(this TeamsContext teamsContext, Guid gameSessionId, Guid teamId)
	{
		var team = await teamsContext.Teams.SingleOrDefaultAsync(
			t => t.GameSessionId == gameSessionId && t.Id == teamId);
		return team ?? throw new InvalidOperationException($"Team with id {teamId} not found");
	}

	public static Task<Team?> FindTeam(this TeamsContext teamsContext, Guid gameSessionId, Guid teamId)
	{
		return teamsContext.Teams.SingleOrDefaultAsync(t => t.GameSessionId == gameSessionId && t.Id == teamId);
	}
}