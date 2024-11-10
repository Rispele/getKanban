using Core.Helpers;
using Core.Services;
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

	public async Task Create(string sessionName, long teamsCount)
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
	
	public async Task Create(string sessionName, long teamsCount, string creatorName)
	{
		var requestContext = RequestContextFactory.Build(Context);

		var session = await gameSessionService.CreateGameSession(requestContext, sessionName, teamsCount);
		var session = await gameSessionService.CreateGameSession(
			requestContext,
			sessionName,
			teamsCount,
			creatorName);

		Console.WriteLine($"{GetGroupId(session.Id)} created");
		
		await AddCurrentConnectionToLobbyGroupAsync(GetGroupId(session.Id));
		await Clients.Caller.SendAsync("Created", session.ToJson());
	}
	
	public async Task Join(string inviteCode)
	{
		var gameSessionId = InviteCodeHelper.SplitInviteCode(inviteCode).sessionId;
		var requestContext = RequestContextFactory.Build(Context);

		var addParticipantResult = await gameSessionService.AddParticipantAsync(requestContext, gameSessionId, inviteCode);
		
		var groupId = GetGroupId(gameSessionId);
		var addParticipantResult = await gameSessionService.AddParticipantAsync(
			requestContext,
			inviteCode,
			userName);

		var groupId = GetGroupId(addParticipantResult.GameSession.Id);
		await AddCurrentConnectionToLobbyGroupAsync(groupId);

		if (!addParticipantResult.Updated)
		{
			return;
		}

		var (teamId, userAdded) = (addParticipantResult.UpdatedTeamId, addParticipantResult.User);
		var a = Clients.OthersInGroup(groupId).ToJson();
		await Clients.All.SendAsync("NotifyJoined", teamId.ToString(), addParticipantResult.User.Id, userAdded.Name);
		await Clients.Group(GetGroupId(addParticipantResult.GameSession.Id)).SendAsync(
			"NotifyJoined",
			teamId.ToString(),
			addParticipantResult.User.Id,
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