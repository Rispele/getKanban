using Core.Helpers;
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

	public async Task LeaveGame(Guid gameSessionId)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		await gameSessionService.RemoveParticipantAsync(requestContext, gameSessionId, currentUser.Id);

		var groupId = GetGroupId(gameSessionId);
		await RemoveCurrentConnectionFromLobbyGroupAsync(groupId);
		await Clients.OthersInGroup(groupId).SendAsync("NotifyLeft", currentUser.Id);
	}
	
	public async Task JoinGame(Guid gameSessionId)
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
	
	public async Task CheckPlayerJoined(string inviteCode)
	{
		var requestContext = RequestContextFactory.Build(Context);
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		Console.WriteLine(currentUser.Name + " checked");
		var session = await gameSessionService.FindGameSession(requestContext, inviteCode, true);
		if (session!.Angels.Participants.Users.All(x => x.Id != currentUser.Id)
		    && session.Teams.All(x => x.Participants.Users.All(p => p.Id != currentUser.Id)))
		{
			await Clients.Caller.SendAsync("NotifyPlayerCheck", false);
		}
		await Clients.Caller.SendAsync("NotifyPlayerCheck", true);
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
	
	private async Task RemoveCurrentConnectionFromLobbyGroupAsync(string groupId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
	}

	private static string GetGroupId(Guid gameSessionId)
	{
		return $"lobby-{gameSessionId}";
	}
}