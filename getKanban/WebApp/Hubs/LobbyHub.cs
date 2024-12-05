using Core.Services.Contracts;
using Domain.Game.Tickets;
using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class LobbyHub : Hub
{
	private readonly IGameSessionService gameSessionService;

	public LobbyHub(IGameSessionService gameSessionService)
	{
		this.gameSessionService = gameSessionService;
	}

	public async Task Join(Guid sessionId)
	{
		await AddCurrentConnectionToLobbyGroupAsync(GetGroupId(sessionId));
	}

	public async Task UpdateName(Guid sessionId, Guid teamId)
	{
		var teamName = await gameSessionService.GetTeamName(sessionId, teamId);
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

	private static string GetGroupId(Guid gameSessionId)
	{
		return $"lobby-{gameSessionId}";
	}
}