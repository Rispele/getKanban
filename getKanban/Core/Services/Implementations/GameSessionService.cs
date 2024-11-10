using Core.DbContexts;
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
	private readonly InviteCodeHelper inviteCodeHelper;

	public GameSessionService(DomainContext context, InviteCodeHelper inviteCodeHelper)
	{
		this.context = context;
		this.inviteCodeHelper = inviteCodeHelper;
	}

	public async Task<GameSessionDto> CreateGameSession(
		RequestContext requestContext,
		string name,
		long teamsCount,
		string creatorName)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());

		user.Name = creatorName;
		var gameSession = new GameSession(user, name, teamsCount);

		context.GameSessions.Add(gameSession);
		await context.SaveChangesAsync();

		return GameSessionDtoConverter.For(ParticipantRole.Creator).Convert(gameSession);
	}

	public async Task<GameSessionDto?> FindGameSession(
		RequestContext requestContext,
		Guid sessionId,
		bool ignorePermissions)
	{
		var session = await context.FindGameSessionsAsync(sessionId);

		if (session is null)
		{
			return null;
		}

		var user = await context.GetUserAsync(requestContext.GetUserId());
		
		var participantRole = ignorePermissions 
			? ParticipantRole.Creator
			: session.EnsureHasAccess(user);

		return GameSessionDtoConverter.For(participantRole).Convert(session);
	}

	public async Task<AddParticipantResult> AddParticipantAsync(
		RequestContext requestContext,
		string inviteCode,
		string userName)
	{
		var gameSessionId = inviteCodeHelper.ResolveGameSessionId(inviteCode);

		var session = await context.GetGameSessionsAsync(gameSessionId);
		var user = await context.GetUserAsync(requestContext.GetUserId());

		user.Name = userName;
		var (teamId, updated) = session.AddByInviteCode(user, inviteCode);

		await context.SaveChangesAsync();

		var participantRole = session.EnsureHasAccess(user);
		var sessionDto = GameSessionDtoConverter.For(participantRole).Convert(session);

		var participantAdded = teamId.Equals(Guid.Empty) && participantRole == ParticipantRole.Angel
			? sessionDto.Angels.Users.Single(x => x.Id == user.Id)
			: sessionDto.Teams
				.Single(t => t.Id == teamId)
				.Participants.Users
				.Single(u => u.Id == user.Id);
		
		return new AddParticipantResult(
			updated,
			sessionDto,
			teamId,
			participantAdded);
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