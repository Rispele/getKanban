using Core.Dtos;
using Core.RequestContexts;
using Core.Services.Implementations;

namespace Core.Services.Contracts;

public interface IGameSessionService
{
	public Task<GameSessionDto> CreateGameSession(RequestContext requestContext, string name, long teamsCount);

	public Task<GameSessionDto?> FindGameSession(RequestContext requestContext, string inviteCode, bool ignorePermissions);

	public Task<AddParticipantResult> AddParticipantAsync(
		RequestContext requestContext,
		string inviteCode);

	public Task StartGameAsync(RequestContext requestContext, Guid gameSessionId);
	
	public Task<Guid?> GetCurrentTeam(RequestContext requestContext, Guid gameSessionId);
	
	public Guid GetTeamInviteId(string inviteCode);
}