﻿using Core.Services.Contracts;
using Domain.Game.Tickets;
using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class TeamSessionHub : Hub
{
	private readonly ITeamService teamService;

	public TeamSessionHub(ITeamService teamService)
	{
		this.teamService = teamService;
	}

	public async Task Join(Guid sessionId, Guid teamId)
	{
		await AddCurrentConnectionToLobbyGroupAsync(GetGroupId(sessionId, teamId));
	}
	
	public async Task ChangePage(Guid gameSessionId, Guid teamId, int pageNumber, int stageNumber)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.Group(groupId).SendAsync("NotifyPageChange", pageNumber, stageNumber);
	}

	public async Task UpdateRole(Guid gameSessionId, Guid teamId, long teamMemberId, string roleTo)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		await Clients.Group(groupId).SendAsync("NotifyUpdateRole", teamMemberId, roleTo);
	}

	public async Task UpdateTicketChoice(Guid gameSessionId, Guid teamId, string ticketId)
	{
		var groupId = GetGroupId(gameSessionId, teamId);
		if (ticketId == TicketDescriptors.AutoRelease.Id)
		{
			await Clients.Group(groupId).SendAsync("NotifyTicketsToReleaseUpdated");
		}
		else
		{
			await Clients.OthersInGroup(groupId).SendAsync("NotifyUpdateTicketChoice", ticketId);
		}
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