using Core.Services;
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

	public async Task Create(string name, long teamsCount)
	{
		var requestContext = RequestContextFactory.Build(Context);

		var session = await gameSessionService.CreateGameSession(requestContext, name, teamsCount);

		await AddCurrentConnectionToLobbyGroupAsync(GetGroupId(session.Id));
		await Clients.Caller.SendAsync("Created", session.ToJson());
	}
	
	public async Task Join(Guid gameSessionId, string inviteCode)
	{
		var requestContext = RequestContextFactory.Build(Context);

		var addParticipantResult = await gameSessionService.AddParticipantAsync(requestContext, gameSessionId, inviteCode);
		
		var groupId = GetGroupId(gameSessionId);
		await AddCurrentConnectionToLobbyGroupAsync(groupId);
		
		await Clients.Caller.SendAsync("Joined", addParticipantResult.GameSession.ToJson());

		if (!addParticipantResult.Updated)
		{
			return;
		}

		var (teamId, userAdded) = (addParticipantResult.UpdatedTeamId, addParticipantResult.User);
		await Clients.OthersInGroup(groupId).SendAsync("NotifyJoined", teamId, userAdded);
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