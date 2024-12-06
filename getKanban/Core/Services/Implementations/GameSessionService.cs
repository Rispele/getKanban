using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.Dtos.Converters;
using Core.Entities;
using Core.Helpers;
using Core.RequestContexts;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Game;
using Domain.Game.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Core.Services.Implementations;

public class GameSessionService : IGameSessionService
{
	private readonly DomainContext context;
	private readonly ConnectionsContext connectionsContext;

	public GameSessionService(
		DomainContext context,
		ConnectionsContext connectionsContext)
	{
		this.context = context;
		this.connectionsContext = connectionsContext;
	}

	public async Task<Guid> CreateGameSession(
		RequestContext requestContext,
		string name,
		long teamsCount)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());
		var gameSession = new GameSession(user, name, teamsCount);

		context.GameSessions.Add(gameSession);
		await context.SaveChangesAsync();

		return gameSession.Id;
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

	public async Task<AddParticipantResult?> AddParticipantAsync(
		RequestContext requestContext,
		string inviteCode)
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

		var (teamId, updated) = session.AddByInviteCode(user, inviteCode);

		await context.SaveChangesAsync();

		var participantRole = session.EnsureHasAnyAccess(user.Id, inviteCode);
		var sessionDto = GameSessionDtoConverter.For(participantRole).Convert(session);

		var participantAdded = teamId.Equals(Guid.Empty) && participantRole == ParticipantRole.Angel
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

	public async Task StartGameAsync(RequestContext requestContext, Guid gameSessionId)
	{
		var userId = requestContext.GetUserId();
		var session = await context.GetGameSessionsAsync(gameSessionId);

		session.EnsureHasAccess(userId);
		await context.CloseRecruitmentAsync(gameSessionId);
		session.Start();

		await context.SaveChangesAsync();
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

	public async Task<UserDto> GetCurrentUser(RequestContext requestContext)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());
		return new UserDto
		{
			Id = user.Id,
			Name = user.Name
		};
	}

	public async Task<CfdGraphDto> GetCfdDataForTeam(Guid sessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);
		var cfd = team.CfdContainers.OrderBy(x => x.Version).ToList();

		var result = new CfdGraphDto();
		var i = 0;
		var initialDayNumber = team.Days.Select(x => x.Number).Min();
		foreach (var day in cfd)
		{
			result.GraphPointsPerLabel["Работа аналитиков"].Add((initialDayNumber + i, day.WithAnalysts!.Value));
			result.GraphPointsPerLabel["Работа разработчиков"].Add((initialDayNumber + i, day.WithProgrammers!.Value));
			result.GraphPointsPerLabel["Работа тестировщиков"].Add((initialDayNumber + i, day.WithTesters!.Value));
			result.GraphPointsPerLabel["Готовы к поставке"].Add((initialDayNumber + i, day.ToDeploy!.Value));
			result.GraphPointsPerLabel["Поставлено"].Add((initialDayNumber + i, day.Released!.Value));
			i++;
		}

		return result;
	}

	public async Task<List<Ticket>> GetTicketsToRelease(Guid sessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);
		var tickets = team.BuildTakenTickets();

		var previousDayNumber = team.CurrentDay.Number - 1;
		return tickets
			.Where(x => x.IsInWork(previousDayNumber))
			.Where(
				x => team.CurrentDay.ReleaseTicketContainer.CanReleaseNotImmediatelyTickets
				  || TicketDescriptors.GetByTicketId(x.id).CanBeReleasedImmediately)
			.ToList();
	}

	public async Task<List<Ticket>> GetBacklogTickets(Guid sessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);

		var tickets = team.BuildTakenTickets();

		var currentDayNumber = team.CurrentDay.Number;
		var previousDayNumber = currentDayNumber - 1;
		var ticketsTakenThisDay = tickets
			.Where(ticket => !ticket.IsTaken(previousDayNumber) && ticket.IsTaken(currentDayNumber))
			.ToList();

		var ticketsNotTaken = TicketDescriptors.AllTicketDescriptors
			.Where(x => tickets.All(t => t.id != x.Id))
			.Select(t => new Ticket(t.Id, int.MaxValue, null));

		return ticketsTakenThisDay.Concat(ticketsNotTaken).ToList();
	}
}