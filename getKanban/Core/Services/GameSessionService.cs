using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.RequestContexts;
using Domain.Game;

namespace Core.Services;

public class GameSessionService : IGameSessionService
{
	private readonly DomainContext context;
	private readonly GameSessionConverter converter;

	public GameSessionService(DomainContext context, GameSessionConverter converter)
	{
		this.context = context;
		this.converter = converter;
	}

	public async Task<GameSessionDto> CreateGameSession(
		RequestContext requestContext,
		string name,
		long teamsCount)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());
		var gameSession = new GameSession(user, name, teamsCount);

		context.GameSessions.Add(gameSession);
		await context.SaveChangesAsync();

		return converter.Convert(gameSession);
	}
	
	public async Task<GameSessionDto?> FindGameSession(Guid sessionId)
	{
		var session = await context.FindGameSessionsAsync(sessionId);
		return session is null
			? null 
			: converter.Convert(session);
	}

	public async Task<AddParticipantResult> AddParticipantAsync(
		RequestContext requestContext,
		Guid gameSessionId,
		string inviteCode)
	{
		var session = await context.GetGameSessionsAsync(gameSessionId);
		var user = await context.GetUserAsync(requestContext.GetUserId());

		var (teamId, updated) = session.AddByInviteCode(user, inviteCode);

		await context.SaveChangesAsync();

		var sessionDto = converter.Convert(session);
		var userAdded = sessionDto.Teams
			.Single(t => t.Id == teamId)
			.Participants.Users
			.Single(u => u.Id == user.Id);
		return new AddParticipantResult(updated, sessionDto, teamId, userAdded);
	}

	public async Task StartGameAsync(RequestContext requestContext, Guid gameSessionId)
	{
		var session = await context.GetGameSessionsAsync(gameSessionId);

		if (!session.HasAccess(requestContext.GetUserId()))
		{
			throw new UnauthorizedAccessException();
		}

		session.Start();

		await context.SaveChangesAsync();
	}
}