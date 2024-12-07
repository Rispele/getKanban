using Core.Dtos;
using Core.Entities;
using Core.RequestContexts;
using Core.Services.Implementations;
using Domain.Game;

namespace Core.Services.Contracts;

public interface IGameSessionService
{
	public Task<Guid> CreateGameSession(RequestContext requestContext, string name, long teamsCount);

	public Task<GameSessionDto?> FindGameSession(
		RequestContext requestContext,
		string inviteCode,
		bool ignorePermissions);

	public Task<GameSessionDto?> FindGameSession(RequestContext requestContext, Guid sessionId, bool ignorePermissions);

	public Task<AddParticipantResult?> AddParticipantAsync(RequestContext requestContext, string inviteCode);
	
	public Task<bool> RemoveParticipantAsync(RequestContext requestContext, Guid sessionId, Guid userId);

	public Task StartGameAsync(RequestContext requestContext, Guid gameSessionId);

	public Task<TeamDto> GetCurrentTeam(RequestContext requestContext, Guid gameSessionId);

	public Guid GetTeamInviteId(string inviteCode);

	public Task UpdateTeamName(Guid sessionId, Guid teamId, string name);

	public Task<string> GetTeamName(Guid sessionId, Guid teamId);

	public Task<UserDto> GetCurrentUser(RequestContext requestContext);

	public Task<List<Ticket>> GetTicketsToRelease(Guid sessionId, Guid teamId);

	public Task<List<Ticket>> GetBacklogTickets(Guid sessionId, Guid teamId);
}