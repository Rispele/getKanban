using Domain.Game.Tickets;
using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class TeamSessionHub : Hub
{
	public async Task Join(Guid sessionId, Guid teamId)
	{
		await AddCurrentConnectionToLobbyGroupAsync(GetGroupId(sessionId, teamId));
	}

	public async Task ChangePage(
		Guid gameSessionId,
		Guid teamId,
		int pageNumber,
		int stageNumber)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.Group(groupId).SendAsync("NotifyPageChange", pageNumber, stageNumber);
	}

	public async Task UpdateRole(
		Guid gameSessionId,
		Guid teamId,
		long teamMemberId,
		string roleTo)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.Group(groupId).SendAsync("NotifyUpdateRole", teamMemberId, roleTo);
	}

	public async Task NotifyTicketReleased(Guid gameSessionId, Guid teamId, string ticketId)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		if (ticketId == TicketDescriptors.AutoRelease.Id)
		{
			await Clients.Group(groupId).SendAsync("NotifyTicketsToReleaseUpdated");
		}
		else
		{
			await Clients.OthersInGroup(groupId).SendAsync("NotifyTicketReleased", ticketId);
		}
	}

	public async Task NotifyTicketTakenToSpringBacklog(Guid gameSessionId, Guid teamId, string ticketId)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.OthersInGroup(groupId).SendAsync("NotifyTicketTakenToSpringBacklog", ticketId);
	}

	public async Task FinishDay(Guid gameSessionId, Guid teamId)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.Group(groupId).SendAsync("NotifyDayFinished", gameSessionId, teamId);
	}

	public async Task ShowGameResult(Guid gameSessionId, Guid teamId)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.Group(groupId).SendAsync("NotifyGameFinished", gameSessionId, teamId);
	}

	public async Task RollbackToDay(Guid gameSessionId, Guid teamId, int dayNumber)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.OthersInGroup(groupId).SendAsync(
			"NotifyRollbackToDay",
			gameSessionId,
			teamId,
			dayNumber);
	}

	public async Task CfdTableUpdate(
		Guid gameSessionId,
		Guid teamId,
		string rowId,
		int value)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.Group(groupId).SendAsync("NotifyCfdTableUpdated", rowId, value);
	}

	public async Task AnotherTeamDiceRoll(
		Guid gameSessionId,
		Guid teamId,
		int diceNumber,
		int scoresNumber,
		int totalScores)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.Group(groupId).SendAsync("NotifyAnotherTeamDiceRolled", diceNumber, scoresNumber, totalScores);
	}

	private async Task AddCurrentConnectionToLobbyGroupAsync(string groupId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
	}

	private static string GetGroupId(Guid gameSessionId, Guid teamId)
	{
		return $"team-session-{gameSessionId}-{teamId}";
	}
}