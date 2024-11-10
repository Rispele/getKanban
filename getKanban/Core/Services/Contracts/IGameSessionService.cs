using Core.Dtos;
using Core.RequestContexts;
using Core.Services.Implementations;

namespace Core.Services.Contracts;

public interface IGameSessionService
{
	public Task<GameSessionDto> CreateGameSession(RequestContext requestContext, string name, long teamsCount, string creatorName);

	public Task<GameSessionDto?> FindGameSession(RequestContext requestContext, Guid sessionId, Guid teamId);

	public Task<AddParticipantResult> AddParticipantAsync(
		RequestContext requestContext,
		Guid gameSessionId,
		string inviteCode,
		string userName);

	public Task StartGameAsync(RequestContext requestContext, Guid gameSessionId);
}