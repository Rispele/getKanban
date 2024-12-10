using Core.RequestContexts;
using Core.Services.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class LobbyHub : Hub
{
	private readonly IGameSessionService gameSessionService;

	public LobbyHub(IGameSessionService gameSessionService)
	{
		this.gameSessionService = gameSessionService;
	}

	public override async Task OnConnectedAsync()
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		if (requestContext.Headers.TryGetValue(RequestContextKeys.SessionId, out var sessionId))
		{
			var sessionToNotifyId = Guid.Parse(sessionId ?? throw new InvalidOperationException());
			await Clients.Group(GetGroupId(sessionToNotifyId)).SendAsync("NotifyConnectionRestore", currentUser.Id);
		}
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		if (requestContext.Headers.TryGetValue(RequestContextKeys.SessionId, out var sessionId))
		{
			var sessionToNotifyId = Guid.Parse(sessionId ?? throw new InvalidOperationException());
			await Clients.Group(GetGroupId(sessionToNotifyId)).SendAsync("NotifyConnectionLost", currentUser.Id);
		}
		await base.OnDisconnectedAsync(exception);
	}

	public async Task CloseLobby(Guid gameSessionId)
	{
		await gameSessionService.CloseGameSession(gameSessionId);

		var groupId = GetGroupId(gameSessionId);
		await RemoveCurrentConnectionFromLobbyGroupAsync(groupId);
		await Clients.OthersInGroup(groupId).SendAsync("NotifyClosed");
	}
	
	public async Task LeaveLobby(Guid gameSessionId)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		await gameSessionService.RemoveParticipantAsync(requestContext, gameSessionId);

		var groupId = GetGroupId(gameSessionId);
		await RemoveCurrentConnectionFromLobbyGroupAsync(groupId);
		await Clients.OthersInGroup(groupId).SendAsync("NotifyLeft", currentUser.Id);
	}
	
	public async Task JoinLobby(Guid gameSessionId)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		try
		{
			var team = await gameSessionService.GetCurrentTeam(requestContext, gameSessionId);
			var groupId = GetGroupId(gameSessionId);
			await AddCurrentConnectionToLobbyGroupAsync(groupId);
			await Clients.OthersInGroup(groupId).SendAsync("NotifyJoined", team.Id, currentUser.Id, currentUser.Name);
		}
		catch (InvalidOperationException e)
		{
		}
	}
	
	public async Task CheckPlayerJoinedSession(string inviteCode)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		var session = await gameSessionService.FindGameSession(requestContext, inviteCode, true);
		
		if (session!.Angels.Participants.Users.All(x => x.Id != currentUser.Id)
		    && session.Teams.All(x => x.Participants.Users.All(p => p.Id != currentUser.Id)))
		{
			await Clients.Caller.SendAsync("NotifyPlayerCheck", false, currentUser.Id);
		}
		await Clients.Caller.SendAsync("NotifyPlayerCheck", true, currentUser.Id);
	}

	public async Task RemovePlayerFromSessionAndLobby(Guid sessionId, Guid userId)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var removed = await gameSessionService.RemoveParticipantAsync(requestContext, sessionId, userId);
		if (removed)
		{
			await Clients.Group(GetGroupId(sessionId)).SendAsync("NotifyLeft", userId);
		}
	}

	public async Task UpdateName(Guid sessionId, Guid teamId, string teamName)
	{
		await gameSessionService.UpdateTeamName(sessionId, teamId, teamName);
		var groupId = GetGroupId(sessionId);

		await Clients.Group(groupId).SendAsync("NotifyRenamed", teamId.ToString(), teamName);
	}

	public async Task StartGame(Guid gameSessionId)
	{
		var requestContext = RequestContextFactory.Build(Context);

		await gameSessionService.StartGameAsync(requestContext, gameSessionId);

		var groupId = GetGroupId(gameSessionId);
		await Clients.Group(groupId).SendAsync("NotifyStarted");
	}
	
	private async Task AddCurrentConnectionToLobbyGroupAsync(string groupId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
	}
	
	private async Task RemoveCurrentConnectionFromLobbyGroupAsync(string groupId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
	}

	public static string GetGroupId(Guid gameSessionId)
	{
		return $"lobby-{gameSessionId}";
	}
}