using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.Dtos.Builders;
using Core.RequestContexts;
using Domain.Game;

namespace Core.Services;

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
		
		var participantRole = session.EnsureHasAccess(user);

		var sessionDto = GameSessionDtoConverter.For(participantRole).Convert(session);
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