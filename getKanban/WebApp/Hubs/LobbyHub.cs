using Core.Helpers;
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

	public async Task Create(string sessionName, long teamsCount)
	{
		var requestContext = RequestContextFactory.Build(Context);

		var session = await gameSessionService.CreateGameSession(requestContext, sessionName, teamsCount);

		await AddCurrentConnectionToLobbyGroupAsync(GetGroupId(session.Id));
		await Clients.Caller.SendAsync("Created", session.ToJson());
	}
	
	public async Task Join(string inviteCode)
	{
		var gameSessionId = InviteCodeHelper.SplitInviteCode(inviteCode).sessionId;
		var requestContext = RequestContextFactory.Build(Context);

		var addParticipantResult = await gameSessionService.AddParticipantAsync(requestContext, gameSessionId, inviteCode);
		
		var groupId = GetGroupId(gameSessionId);
		await AddCurrentConnectionToLobbyGroupAsync(groupId);

		if (!addParticipantResult.Updated)
		{
			return;
		}

		var (teamId, userAdded) = (addParticipantResult.UpdatedTeamId, addParticipantResult.User);
		var a = Clients.OthersInGroup(groupId).ToJson();
		await Clients.All.SendAsync("NotifyJoined", teamId.ToString(), addParticipantResult.User.Id, userAdded.Name);
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