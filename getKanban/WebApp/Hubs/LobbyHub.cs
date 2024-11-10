using Core.Helpers;
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
		Console.WriteLine("Connected " + Context.ConnectionId);
		await base.OnConnectedAsync();
	}
	
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		Console.WriteLine("Disconnected " + Context.ConnectionId);
		await base.OnDisconnectedAsync(exception);
	}
	
	public async Task Create(string sessionName, long teamsCount)
	{
		var requestContext = RequestContextFactory.Build(Context);

		var session = await gameSessionService.CreateGameSession(requestContext, sessionName, teamsCount);

		Console.WriteLine($"{GetGroupId(session.Id)} created");
		
		await AddCurrentConnectionToLobbyGroupAsync(GetGroupId(session.Id));
		await Clients.Caller.SendAsync("Created", session.ToJson());
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

	public async Task StartGame(Guid gameSessionId)
	{
		var requestContext = RequestContextFactory.Build(Context);

		await gameSessionService.StartGameAsync(requestContext, gameSessionId);

		await Clients.Group(GetGroupId(gameSessionId)).SendAsync("NotifyStarted");
	}

	private async Task AddCurrentConnectionToLobbyGroupAsync(string groupId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
	}

	private static string GetGroupId(Guid gameSessionId)
	{
		return $"lobby-{gameSessionId}";
	}
}