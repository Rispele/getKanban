﻿using Core.Dtos;
using Core.RequestContexts;
using Core.Services.Implementations;

namespace Core.Services.Contracts;

public interface IGameSessionService
{
	public Task<Guid> CreateGameSession(RequestContext requestContext, string name, long teamsCount);

	public Task<GameSessionDto?> FindGameSession(
		RequestContext requestContext,
		string inviteCode,
		bool ignorePermissions);

	public Task<AddParticipantResult?> AddParticipantAsync(RequestContext requestContext, string inviteCode);
	
	public Task<bool> RemoveParticipantAsync(RequestContext requestContext, Guid sessionId);

	public Task StartGameAsync(RequestContext requestContext, Guid gameSessionId);

	public Task<TeamDto> GetCurrentTeam(RequestContext requestContext, Guid gameSessionId);

	public Task UpdateTeamName(Guid sessionId, Guid teamId, string name);

	public Task<string> GetTeamName(Guid sessionId, Guid teamId);

	public Task<UserDto> GetCurrentUser(RequestContext requestContext);
}