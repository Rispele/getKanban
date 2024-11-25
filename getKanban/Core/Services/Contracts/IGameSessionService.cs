using Core.Dtos;
using Core.Entities;
using Core.RequestContexts;
using Core.Services.Implementations;
using Domain.Game;
using Domain.Game.Teams;

namespace Core.Services.Contracts;

public interface IGameSessionService
{
	public Task<GameSessionDto> CreateGameSession(RequestContext requestContext, string name, long teamsCount);

	public Task<GameSessionDto?> FindGameSession(RequestContext requestContext, string inviteCode, bool ignorePermissions);

	public Task<AddParticipantResult> AddParticipantAsync(
		RequestContext requestContext,
		string inviteCode);

	public Task StartGameAsync(RequestContext requestContext, Guid gameSessionId);
	
	public Task<TeamDto?> GetCurrentTeam(RequestContext requestContext, Guid gameSessionId);
	
	public Guid GetTeamInviteId(string inviteCode);
	
	public Task UpdateTeamName(Guid sessionId, Guid teamId, string name);
	
	public Task<string> GetTeamName(Guid sessionId, Guid teamId);

	public Task<UserDto> GetCurrentUser(RequestContext requestContext);

	public Task<HubConnection?> GetCurrentConnection(Guid userId);

	public Task<HubConnection?> SaveCurrentConnection(Guid userId, string lobbyId, string hubConnectionId);

	public Task RemoveConnection(Guid userId);
}