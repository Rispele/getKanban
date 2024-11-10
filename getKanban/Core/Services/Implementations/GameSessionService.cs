﻿using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.Dtos.Converters;
using Core.Helpers;
using Core.RequestContexts;
using Core.Services.Contracts;
using Domain.Game;

namespace Core.Services.Implementations;

public class GameSessionService : IGameSessionService
{
	private readonly DomainContext context;

	public GameSessionService(DomainContext context)
	{
		this.context = context;
	}

	public async Task<GameSessionDto> CreateGameSession(
		RequestContext requestContext,
		string name,
		long teamsCount,
		string creatorName)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());
		await context.SetUserName(user, creatorName);
		var gameSession = new GameSession(user, name, teamsCount);

		context.GameSessions.Add(gameSession);
		await context.SaveChangesAsync();

		return GameSessionDtoConverter.For(ParticipantRole.Creator).Convert(gameSession);
	}
	
	public async Task<GameSessionDto?> FindGameSession(RequestContext requestContext, Guid sessionId, Guid teamId)
	{
		var session = await context.FindGameSessionsAsync(sessionId);
		var user = await context.GetUserAsync(requestContext.GetUserId());
		var participantRole = session?.EnsureHasAccess(user!, sessionId, teamId);

		if (session is not null && participantRole is null)
		{
			throw new InvalidOperationException("User has not access to this session.");
		}

		if (session is null)
		{
			return null;
		}
		return GameSessionDtoConverter.For(participantRole!.Value).Convert(session);
	}

	public async Task<AddParticipantResult> AddParticipantAsync(
		RequestContext requestContext,
		Guid gameSessionId,
		string inviteCode,
		string userName)
	{
		var session = await context.GetGameSessionsAsync(gameSessionId);
		var user = await context.GetUserAsync(requestContext.GetUserId());
		await context.SetUserName(user, userName);

		var (teamId, updated) = session.AddByInviteCode(user, inviteCode);
		await context.SaveChangesAsync();

		var inviteTeamId = Guid.Parse(InviteCodeHelper.SplitInviteCode(inviteCode).teamId);
		var participantRole = session.EnsureHasAccess(user, gameSessionId, inviteTeamId);
		var sessionDto = GameSessionDtoConverter.For(participantRole).Convert(session);
		
		if (teamId.Equals(Guid.Empty) && participantRole == ParticipantRole.Angel)
		{
			var angelAdded = sessionDto.Angels.Users
				.Single(x => x.Id == user.Id);
			return new AddParticipantResult(updated, sessionDto, inviteTeamId, angelAdded);
		}
		
		var userAdded = sessionDto.Teams
			.Single(t => t.Id == teamId)
			.Participants.Users
			.Single(u => u.Id == user.Id);
		return new AddParticipantResult(updated, sessionDto, teamId, userAdded);
	}

	public async Task StartGameAsync(RequestContext requestContext, Guid gameSessionId)
	{
		var session = await context.GetGameSessionsAsync(gameSessionId);
		var user = await context.GetUserAsync(requestContext.GetUserId());
		
		session.EnsureHasAccess(user);
		session.Start();

		await context.SaveChangesAsync();
	}
}