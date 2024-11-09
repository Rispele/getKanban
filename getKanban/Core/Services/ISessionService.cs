using Domain.Game.Teams;

namespace Core;

public interface ISessionService
{
	public Task<Guid?> TryCreateSession(string sessionName, long teamsCount);
	public Task<(List<Team>, string)> TryGetSession(Guid sessionId);

	public Task<bool> SessionExist(Guid sessionId);
}