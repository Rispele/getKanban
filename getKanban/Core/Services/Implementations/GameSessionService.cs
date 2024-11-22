using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.Dtos.Converters;
using Core.Helpers;
using Core.RequestContexts;
using Core.Services.Contracts;
using Domain.Game;
using Domain.Game.Teams;

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
		long teamsCount)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());
		var gameSession = new GameSession(user, name, teamsCount);

		context.GameSessions.Add(gameSession);
		await context.SaveChangesAsync();

		return GameSessionDtoConverter.For(ParticipantRole.Creator).Convert(gameSession);
	}

	public async Task<GameSessionDto?> FindGameSession(
		RequestContext requestContext,
		string inviteCode,
		bool ignorePermissions)
	{
		var sessionId = InviteCodeHelper.ResolveGameSessionId(inviteCode);
		var session = await context.FindGameSessionsAsync(sessionId);

		if (session is null)
		{
			return null;
		}

		var user = await context.GetUserAsync(requestContext.GetUserId());

		ParticipantRole participantRole;
		if (ignorePermissions)
		{
			participantRole = ParticipantRole.Creator;
		}
		else
		{
			participantRole = session.EnsureHasAnyAccess(user, inviteCode);
		}

		return GameSessionDtoConverter.For(participantRole).Convert(session);
	}

	public async Task<AddParticipantResult> AddParticipantAsync(
		RequestContext requestContext,
		string inviteCode)
	{
		var gameSessionId = InviteCodeHelper.ResolveGameSessionId(inviteCode);

		var session = await context.GetGameSessionsAsync(gameSessionId);
		var user = await context.GetUserAsync(requestContext.GetUserId());

		var (teamId, updated) = session.AddByInviteCode(user, inviteCode);

		await context.SaveChangesAsync();
		
		var participantRole = session.EnsureHasAnyAccess(user, inviteCode);
		var sessionDto = GameSessionDtoConverter.For(participantRole).Convert(session);

		var participantAdded = teamId.Equals(Guid.Empty) && participantRole == ParticipantRole.Angel
			? sessionDto.Angels.Participants.Users
				.Single(x => x.Id == user.Id)
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

	public async Task<TeamDto?> GetCurrentTeam(RequestContext requestContext, Guid gameSessionId)
	{
		var session = await context.GetGameSessionsAsync(gameSessionId);
		var user = await context.GetUserAsync(requestContext.GetUserId());
		
		var angels =
			session.Angels.Participants.SingleOrDefault(x => x.User.Id == user.Id) is null
				? null
				: session.Angels;
		var team = session.Teams
			.SingleOrDefault(
				x => x.Players.Participants
					.SingleOrDefault(p => p.User.Id == user.Id) is not null);
		
		return angels is not null
			? TeamDtoConverter.For(ParticipantRole.Creator).ConvertAngels(angels)
			: TeamDtoConverter.For(ParticipantRole.Player).Convert(team);
	}

	public Guid GetTeamInviteId(string inviteCode)
	{
		return InviteCodeHelper.SplitInviteCode(inviteCode).teamId;
	}

	public async Task UpdateTeamName(Guid sessionId, Guid teamId, string name)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);
		team.Name = name;
		await context.SaveChangesAsync();
	}
	
	public async Task<string> GetTeamName(Guid sessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);
		return team.Name;
	}
}