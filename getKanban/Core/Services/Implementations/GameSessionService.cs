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
		var sessionId = InviteCodeHelper.ResolveGameSessionId(inviteCode);
		var session = await context.FindGameSessionsAsync(sessionId);

		if (session is null)
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
		return await FindGameSession(requestContext, $"{sessionId}.{Guid.NewGuid()}", ignorePermissions);
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

	public async Task<Guid?> FindCurrentSessionId(RequestContext requestContext)
	{
		var user = await context.GetUserAsync(requestContext.GetUserId());
		var sessions = context.GameSessions.ToList();
		foreach (var session in sessions)
		{
			var teamsParticipants = session.Teams.ToList().SelectMany(x => x.Players.Participants);
			var angelsParticipants = session.Angels.Participants;

			var teamParticipant = teamsParticipants.SingleOrDefault(x => x.User.Id == user.Id);
			if (teamParticipant != default)
			{
				return session.Id;
			}

			var angelsParticipant = angelsParticipants.SingleOrDefault(x => x.User.Id == user.Id);
			if (angelsParticipant != default)
			{
				return session.Id;
			}
		}

		return null;
	}

	public async Task<CfdGraphDto> GetCfdDataForTeam(Guid sessionId, Guid teamId)
	{
		var session = context.GameSessions.SingleOrDefault(x => x.Id == sessionId);
		var team = session!.Teams.SingleOrDefault(x => x.Id == teamId);
		var cfd = team!.CfdContainers.ToList();

		var result = new CfdGraphDto();
		var i = 0;
		foreach (var day in cfd)
		{
			if (!result.GraphPointsPerLabel.ContainsKey("Работа аналитиков"))
				result.GraphPointsPerLabel["Работа аналитиков"] = new List<(int, int)>();
			if (!result.GraphPointsPerLabel.ContainsKey("Работа разработчиков"))
				result.GraphPointsPerLabel["Работа разработчиков"] = new List<(int, int)>();
			if (!result.GraphPointsPerLabel.ContainsKey("Работа тестировщиков"))
				result.GraphPointsPerLabel["Работа тестировщиков"] = new List<(int, int)>();
			if (!result.GraphPointsPerLabel.ContainsKey("Готовы к поставке"))
				result.GraphPointsPerLabel["Готовы к поставке"] = new List<(int, int)>();
			if (!result.GraphPointsPerLabel.ContainsKey("Поставлено"))
				result.GraphPointsPerLabel["Поставлено"] = new List<(int, int)>();
			
			result.DaysToShow.Add(9 + i);
			result.TotalTasksToShow.Add(day.WithAnalysts!.Value);
			result.TotalTasksToShow.Add(day.WithProgrammers!.Value);
			result.TotalTasksToShow.Add(day.WithTesters!.Value);
			result.TotalTasksToShow.Add(day.ToDeploy!.Value);
			result.TotalTasksToShow.Add(day.Released!.Value);
			result.GraphPointsPerLabel["Работа аналитиков"].Add((9 + i, day.WithAnalysts!.Value));
			result.GraphPointsPerLabel["Работа разработчиков"].Add((9 + i, day.WithProgrammers!.Value));
			result.GraphPointsPerLabel["Работа тестировщиков"].Add((9 + i, day.WithTesters!.Value));
			result.GraphPointsPerLabel["Готовы к поставке"].Add((9 + i, day.ToDeploy!.Value));
			result.GraphPointsPerLabel["Поставлено"].Add((9 + i, day.Released!.Value));
			i++;
		}

		return result;
	}

	public async Task<List<string>> GetReleaseTickets(Guid sessionId, Guid teamId)
	{
		var session = context.GameSessions.SingleOrDefault(x => x.Id == sessionId);
		var team = session!.Teams.SingleOrDefault(x => x.Id == teamId);
		var tickets = team!.BuildTakenTickets();
		return tickets.Select(x => x.id).ToList();
	}

	public async Task<List<string>> GetBacklogTickets(Guid sessionId, Guid teamId)
	{
		var session = context.GameSessions.SingleOrDefault(x => x.Id == sessionId);
		var team = session!.Teams.SingleOrDefault(x => x.Id == teamId);
		var tickets = team!.BuildTakenTickets().ToList();
		return TicketDescriptors.AllTicketDescriptors
			.Select(x => x.Id)
			.Except(tickets.Select(x => x.id))
			.ToList();
	}
	

	public async Task<HubConnection?> FindCurrentConnection(Guid userId)
	{
		return await connectionsContext.FindCurrentConnection(userId);
	}

	public async Task<HubConnection> SaveCurrentConnection(Guid userId, string lobbyId, string hubConnectionId)
	{
		var connection = await connectionsContext.FindCurrentConnection(userId);
		if (connection is null)
		{
			connection = new HubConnection
			{
				UserId = userId,
				LobbyId = lobbyId,
				HubConnectionId = hubConnectionId
			};
			connectionsContext.HubConnections.Add(connection);
			await connectionsContext.SaveChangesAsync();
			return connection;
		}

		connection.LobbyId = lobbyId;
		connection.HubConnectionId = hubConnectionId;
		await connectionsContext.SaveChangesAsync();
		return connection;
	}

	public async Task RemoveConnection(Guid userId)
	{
		await connectionsContext.HubConnections
			.AsNoTracking()
			.Where(x => x.UserId == userId)
			.ExecuteDeleteAsync();
		await connectionsContext.SaveChangesAsync();
	}
}