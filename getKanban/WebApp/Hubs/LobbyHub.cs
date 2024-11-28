using Core.Services.Contracts;
using Domain.Serializers;
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
		var connection = await gameSessionService.GetCurrentConnection(currentUser.Id);
		if (connection is null)
		{
			return;
		}
		if (connection.HubConnectionId != Context.ConnectionId)
		{
			await gameSessionService.SaveCurrentConnection(currentUser.Id, connection.LobbyId, Context.ConnectionId);
			await RemoveCurrentConnectionFromLobbyGroupAsync(connection.LobbyId);
			await AddCurrentConnectionToLobbyGroupAsync(connection.LobbyId);
		}
		
		Console.WriteLine("Connected " + Context.ConnectionId);
		await base.OnConnectedAsync();
	}
	
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		var connection = await gameSessionService.GetCurrentConnection(currentUser.Id);
		if (connection is not null)
		{
			await RemoveCurrentConnectionFromLobbyGroupAsync(connection.LobbyId);
		
			Console.WriteLine("Disconnected " + Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}
	}
	
	public async Task Create(Guid sessionId)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var session = await gameSessionService.FindGameSession(requestContext, sessionId, ignorePermissions: true);
		Console.WriteLine($"{GetGroupId(session!.Id)} created");
		await AddCurrentConnectionToLobbyGroupAsync(GetGroupId(session!.Id));
		await Clients.Caller.SendAsync("Created", session!.Angels.Participants.InviteCode);
	}
	
	public async Task Join(string inviteCode)
	{
		var requestContext = RequestContextFactory.Build(Context);
		
		var addParticipantResult = await gameSessionService.AddParticipantAsync(
			requestContext,
			inviteCode);

		var groupId = GetGroupId(addParticipantResult.GameSession.Id);
		await AddCurrentConnectionToLobbyGroupAsync(groupId);

		if (!addParticipantResult.Updated)
		{
			return;
		}

		var (teamId, userAdded) = (addParticipantResult.UpdatedTeamId, addParticipantResult.User);
		Console.WriteLine($"{groupId} notified");
		await Clients.Group(groupId).SendAsync(
			"NotifyJoined",
			teamId.ToString(),
			addParticipantResult.User.Id.ToString(),
			userAdded.Name);
	}

	public async Task UpdateName(Guid sessionId, Guid teamId)
	{
		var teamName = await gameSessionService.GetTeamName(sessionId, teamId);
		var groupId = GetGroupId(sessionId);
		
		Console.WriteLine($"{groupId} notified");
		await Clients.All.SendAsync(
			"NotifyRenamed",
			teamId.ToString(),
			teamName);
	}

	public async Task StartGame(Guid gameSessionId)
	{
		var requestContext = RequestContextFactory.Build(Context);

		await gameSessionService.StartGameAsync(requestContext, gameSessionId);

		await Clients.Group(GetGroupId(gameSessionId)).SendAsync("NotifyStarted");
	}

	public async Task ChangePage(Guid gameSessionId, int pageNumber, int stageNumber)
	{
		await Clients.Group(GetGroupId(gameSessionId)).SendAsync("NotifyPageChange", pageNumber, stageNumber);
	}

	public async Task UpdateRole(Guid gameSessionId, long teamMemberId, string roleTo)
	{
		await Clients.Group(GetGroupId(gameSessionId)).SendAsync("NotifyUpdateRole", teamMemberId, roleTo);
	}
	
	public async Task UpdateTicketChoice(Guid gameSessionId, string ticketId)
	{
		await Clients.OthersInGroup(GetGroupId(gameSessionId)).SendAsync("NotifyUpdateTicketChoice", ticketId);
	}

	private async Task AddCurrentConnectionToLobbyGroupAsync(string groupId)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		await gameSessionService.SaveCurrentConnection(currentUser.Id, groupId, Context.ConnectionId);
		
		await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
	}

	private async Task RemoveCurrentConnectionFromLobbyGroupAsync(string groupId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
	}

	private static string GetGroupId(Guid gameSessionId)
	{
		return $"lobby-{gameSessionId}";
	}
}