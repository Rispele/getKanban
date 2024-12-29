using Core.DbContexts.Extensions;
using Core.DbContexts.Helpers;
using Core.Dtos;
using Core.Dtos.Converters;
using Core.Helpers;
using Core.RequestContexts;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Game;

namespace Core.Services.Implementations;

public class GameSessionService : IGameSessionService
{
	private readonly DomainContext context;

	public GameSessionService(DomainContext context)
	{
		this.context = context;
	}

	public async Task<List<GameSessionInfoDto>> GetUserRelatedSessions(Guid userId)
	{
		var sessions = context.GameSessions.ToList()
			.Select(x => GameSessionDtoConverter.For(ParticipantRole.Player).Convert(x)).ToList();
		var userRelatedSessions = sessions
			.Where(
				x => x.Teams.Any(t => t.Participants.Users.Any(u => u.Id == userId))
				  || x.Angels.Participants.Users.Any(u => u.Id == userId))
			.ToList();
		return userRelatedSessions.Select(
				x =>
					new GameSessionInfoDto
					{
						GameSessionId = x.Id,
						TeamId = x.Teams.Any(t => t.Participants.Users.Any(u => u.Id == userId))
							? x.Teams.First(t => t.Participants.Users.Any(u => u.Id == userId)).Id
							: x.Angels.Id,
						GameSessionName = x.Name,
						TeamsCount = x.Teams.Count,
						GameSessionStatus = x.IsRecruitmentFinished
							? x.Teams.All(t => t.IsTeamSessionEnded) ? "Завершена" : "В процессе"
							: "Набор игроков",
						RequesterParticipantRole = x.Teams.Any(t => t.Participants.Users.Any(u => u.Id == userId))
							? ParticipantRole.Player
							: ParticipantRole.Angel
					})
			.ToList();
	}

	public async Task<Guid> CreateGameSession(
		RequestContext requestContext,
		string name,
		long teamsCount)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());
		var gameSession = new GameSession(user, name, teamsCount);

		context.GameSessions.Add(gameSession);
		await context.TrySaveChangesAsync();

		return gameSession.Id;
	}

	public async Task CloseGameSession(Guid sessionId)
	{
		context.GameSessions.Remove(context.GameSessions.Single(x => x.Id == sessionId));
		await context.TrySaveChangesAsync();
	}

	public async Task<GameSessionDto?> FindGameSession(
		RequestContext requestContext,
		string inviteCode,
		bool ignorePermissions)
	{
		if (!InviteCodeHelper.ValidateInviteCode(inviteCode))
		{
			return null;
		}

		var sessionId = InviteCodeHelper.ResolveGameSessionId(inviteCode);
		var session = await context.FindGameSessionsAsync(sessionId);

		if (session?.EnsureHasInviteCodeAccess(inviteCode) is null)
		{
			return null;
		}

		var userId = requestContext.GetUserId();
		var participantRole = ignorePermissions
			? ParticipantRole.Creator
			: session.EnsureHasAnyAccess(userId, inviteCode);

		return GameSessionDtoConverter.For(participantRole).Convert(session);
	}

	public async Task<AddParticipantResult?> AddParticipantAsync(RequestContext requestContext, string inviteCode)
	{
		if (!InviteCodeHelper.ValidateInviteCode(inviteCode))
		{
			return null;
		}

		var gameSessionId = InviteCodeHelper.ResolveGameSessionId(inviteCode);

		var session = await context.FindGameSessionsAsync(gameSessionId);
		if (session?.EnsureHasInviteCodeAccess(inviteCode) is null)
		{
			return null;
		}

		var user = await context.GetUserAsync(requestContext.GetUserId());

		await RemoveParticipantAsync(requestContext, gameSessionId, user.Id);
		var (teamId, updated) = session.AddByInviteCode(user, inviteCode);
		await context.TrySaveChangesAsync();

		var participantRole = session.EnsureHasAnyAccess(user.Id, inviteCode);
		var sessionDto = GameSessionDtoConverter.For(participantRole).Convert(session);

		var participantAdded = teamId.Equals(sessionDto.Angels.Id) && participantRole == ParticipantRole.Angel
			? sessionDto.Angels.Participants.Users.Single(x => x.Id == user.Id)
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

	public async Task<bool> RemoveParticipantAsync(RequestContext requestContext, Guid sessionId, Guid? userId = null)
	{
		var session = await context.FindGameSessionsAsync(sessionId);
		if (session is null)
		{
			return false;
		}

		var user = await context.GetUserAsync(userId ?? requestContext.GetUserId());

		var angelsRemoved = session.Angels.RemoveParticipant(user);
		var teamsRemoved = session.Teams.Any(x => x.Players.RemoveParticipant(user));

		await context.TrySaveChangesAsync();

		return angelsRemoved || teamsRemoved;
	}

	public async Task StartGameAsync(RequestContext requestContext, Guid gameSessionId)
	{
		var userId = requestContext.GetUserId();
		var session = await context.GetGameSessionsAsync(gameSessionId);

		session.EnsureHasAccess(userId);
		await context.CloseRecruitmentAsync(gameSessionId);
		session.Start();

		await context.TrySaveChangesAsync();
	}

	public async Task<TeamDto> GetCurrentTeam(RequestContext requestContext, Guid gameSessionId)
	{
		var userId = requestContext.GetUserId();
		var session = await context.GetGameSessionsAsync(gameSessionId);

		var participantRole = session.EnsureHasAccess(userId);
		return participantRole switch
		{
			ParticipantRole.Creator or ParticipantRole.Angel or ParticipantRole.Creator | ParticipantRole.Angel =>
				TeamDtoConverter.For(ParticipantRole.Creator).ConvertAngels(session.Angels),
			ParticipantRole.Player =>
				TeamDtoConverter.For(ParticipantRole.Player).Convert(session.FindUserTeam(userId)!),
			null => throw new InvalidOperationException($"No team found for user {userId}"),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public async Task<bool> UpdateTeamName(Guid sessionId, Guid teamId, string name)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);
		team.Name = name;
		var result = await context.TrySaveChangesAsync();
		return result > 0;
	}

	public async Task<string> GetTeamName(Guid sessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);
		return team.Name;
	}

	public async Task<UserDto> GetCurrentUser(RequestContext requestContext)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());
		return new UserDto
		{
			Id = user.Id,
			Name = user.Name
		};
	}
	
	public async Task<bool> CheckValidCfd(RequestContext requestContext, Guid gameSessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(gameSessionId, teamId);
		//var isEndDayAwaited = team.CurrentlyAwaitedCommands.Any(x => x.CommandType is DayCommandType.EndDay);
		var isCfdValid = team.IsCurrentDayCfdValid();
		return isCfdValid;
	}

	public async Task<GameSessionDto?> FindGameSession(
		RequestContext requestContext,
		Guid sessionId,
		bool ignorePermissions)
	{
		var userId = requestContext.GetUserId();
		var session = await context.FindGameSessionsAsync(sessionId);
		if (session?.EnsureHasAccess(userId) is null)
		{
			return null;
		}

		var participantRole = ignorePermissions
			? ParticipantRole.Creator
			: session.EnsureHasAccess(userId);

		return GameSessionDtoConverter.For(participantRole!.Value).Convert(session);
	}
}