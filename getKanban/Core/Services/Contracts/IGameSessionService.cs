using Core.Dtos;
using Core.RequestContexts;
using Core.Services.Implementations;

namespace Core.Services.Contracts;

public interface IGameSessionService
{
	public Task<List<GameSessionInfoDto>> GetUserRelatedSessions(Guid userId);

	public Task<Guid> CreateGameSession(RequestContext requestContext, string name, long teamsCount);

	public Task CloseGameSession(Guid sessionId);

	public Task<GameSessionDto?> FindGameSession(
		RequestContext requestContext,
		string inviteCode,
		bool ignorePermissions);

	public Task<AddParticipantResult?> AddParticipantAsync(RequestContext requestContext, string inviteCode);

	public Task<bool> RemoveParticipantAsync(RequestContext requestContext, Guid sessionId, Guid? userId = null);

	public Task StartGameAsync(RequestContext requestContext, Guid gameSessionId);

	public Task<TeamDto> GetCurrentTeam(RequestContext requestContext, Guid gameSessionId);

	public Task<bool> UpdateTeamName(Guid sessionId, Guid teamId, string name);

	public Task<string> GetTeamName(Guid sessionId, Guid teamId);

	public Task<UserDto> GetCurrentUser(RequestContext requestContext);

	public Task<bool> ShouldLockTestersForTeam(
		RequestContext requestContext,
		Guid gameSessionId,
		Guid teamId,
		int dayNumber);

	public Task<bool> CheckValidCfd(RequestContext requestContext, Guid gameSessionId, Guid teamId);
}