using Domain.Game;
using Domain.Game.Teams;
using Domain.Users;

namespace Core.Services;

public class SessionService : ISessionService
{
	private GameSessionsContext context;

	public SessionService(GameSessionsContext context)
	{
		this.context = context;
	}
	
	public async Task<Guid?> TryCreateSession(string sessionName, long teamsCount)
	{
		var gameSession = new GameSession(new User("Admin"), sessionName, teamsCount);
		context.GameSessions.Add(gameSession);
		await context.SaveChangesAsync();
		return gameSession.Id;
	}

	public async Task<(List<Team>, string)> TryGetSession(Guid sessionId)
	{
		var sessions = context.GameSessions.ToList();
		var session = await context.GameSessions
			.FirstOrDefaultAsync(x => x.Id == sessionId);
		var (teams, sessionName) = (session?.Teams, session?.Name);
		if (teams != null && sessionName != null)
		{
			return (teams.ToList(), sessionName);
		}

		return ([], string.Empty);
	}

	public async Task<bool> SessionExist(Guid sessionId)
	{
		return await context.GameSessions.AnyAsync(x => x.Id == sessionId);
	}
}