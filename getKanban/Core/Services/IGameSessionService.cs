using Core.Dtos;
using Core.RequestContexts;
using Domain.Game.Teams;

namespace Core.Services;

public interface IGameSessionService
{
	public Task<GameSessionDto> CreateGameSession(RequestContext requestContext, string name, long teamsCount);

	public Task<GameSessionDto?> FindGameSession(Guid sessionId);

	public Task<AddParticipantResult> AddParticipantAsync(
		RequestContext requestContext,
		Guid gameSessionId,
		string inviteCode);

	public Task StartGameAsync(RequestContext requestContext, Guid gameSessionId);
}