using Core.RequestContexts;
using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class LobbyHub : Hub
{
	public override async Task OnConnectedAsync()
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUserId = requestContext.GetUserId();
		if (requestContext.Headers.TryGetValue(RequestContextKeys.SessionId, out var sessionId))
		{
			var sessionToNotifyId = Guid.Parse(sessionId ?? throw new InvalidOperationException());
			await Clients.Group(GetGroupId(sessionToNotifyId)).SendAsync("NotifyConnectionUpdate", currentUserId, true);
		}
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUserId = requestContext.GetUserId();
		if (requestContext.Headers.TryGetValue(RequestContextKeys.SessionId, out var sessionId))
		{
			var sessionToNotifyId = Guid.Parse(sessionId ?? throw new InvalidOperationException());
			await Clients.Group(GetGroupId(sessionToNotifyId)).SendAsync("NotifyConnectionUpdate", currentUserId, false);
		}
		await base.OnDisconnectedAsync(exception);
	}

	public async Task JoinGame(Guid gameSessionId, Guid teamId, Guid joinedUserId, string joinedUserName)
	{
		var groupId = GetGroupId(gameSessionId);
		await AddCurrentConnectionToLobbyGroupAsync(groupId);
		await Clients.OthersInGroup(groupId).SendAsync("NotifyJoinGame", teamId, joinedUserId, joinedUserName);
	}
	
	public async Task RemoveUser(Guid gameSessionId, Guid userId)
	{
		var groupId = GetGroupId(gameSessionId);
		await Clients.Group(groupId).SendAsync("NotifyLeaveGame", userId);
	}

	public async Task UpdateTeamName(Guid gameSessionId, Guid teamId, string teamName)
	{
		var groupId = GetGroupId(gameSessionId);
		await Clients.Group(groupId).SendAsync("NotifyUpdateTeamName", teamId.ToString(), teamName);
	}

	public async Task StartGame(Guid gameSessionId)
	{
		var groupId = GetGroupId(gameSessionId);
		await Clients.Group(groupId).SendAsync("NotifyStartGame");
	}
	
	public async Task CloseGame(Guid gameSessionId)
	{
		var groupId = GetGroupId(gameSessionId);
		await RemoveCurrentConnectionFromLobbyGroupAsync(groupId);
		await Clients.OthersInGroup(groupId).SendAsync("NotifyCloseGame");
	}
	
	public async Task LeaveGame(Guid gameSessionId, Guid leftUserId)
	{
		var groupId = GetGroupId(gameSessionId);
		await RemoveCurrentConnectionFromLobbyGroupAsync(groupId);
		await Clients.OthersInGroup(groupId).SendAsync("NotifyLeaveGame", leftUserId);
	}

	public async Task ConnectExplicit(Guid gameSessionId)
	{
		var groupId = GetGroupId(gameSessionId);
		await AddCurrentConnectionToLobbyGroupAsync(groupId);
	}

	public async Task TeamGameResultReveal(Guid gameSessionId, Guid teamId)
	{
		var groupId = GetGroupId(gameSessionId);
		await Clients.Group(groupId).SendAsync("NotifyResult", teamId);
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