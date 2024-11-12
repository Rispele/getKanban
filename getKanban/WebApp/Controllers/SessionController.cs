using Core.Helpers;
using Core.Services.Contracts;
using Domain.Game.Teams;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApp.Controllers;

[Route("session")]
public class SessionController : Controller
{
	private readonly IGameSessionService gameSessionService;

	public SessionController(IGameSessionService gameSessionService)
	{
		this.gameSessionService = gameSessionService;
	}

	[HttpGet]
	[Route("")]
	public async Task<IActionResult> EditSession(string invite)
	{
		var session = await gameSessionService.FindGameSession(
			RequestContextFactory.Build(Request),
			invite,
			ignorePermissions: false);
		
		return View(session);
	}

	[HttpGet]
	[Route("join")]
	public IActionResult JoinSession()
	{
		return View();
	}

	[HttpGet]
	[Route("create")]
	public IActionResult CreateSession()
	{
		return View();
	}

	[HttpGet]
	[Route("check")]
	public async Task<bool> CheckForOpenedGame(string invite)
	{
		return await gameSessionService.FindGameSession(
			RequestContextFactory.Build(Request),
			invite,
			ignorePermissions: false) != null;
	}

	[HttpGet]
	[Route("get-current-team")]
	public async Task<string?> GetCurrentTeam(Guid sessionId)
	{
		var team = await gameSessionService.GetCurrentTeam(RequestContextFactory.Build(Request), sessionId);
		return team is null ? string.Empty : JsonConvert.SerializeObject(team);
	}
	
	[HttpGet]
	[Route("get-current-angels")]
	public async Task<string?> GetCurrentAngels(Guid sessionId)
	{
		var team = await gameSessionService.GetCurrentAngels(RequestContextFactory.Build(Request), sessionId);
		return team is null ? string.Empty : JsonConvert.SerializeObject(team);
	}
	
	[HttpGet]
	[Route("get-team-invite")]
	public Guid GetTeamInviteId(string invite)
	{
		return gameSessionService.GetTeamInviteId(invite);
	}
	
	[HttpGet]
	[Route("update-team-name")]
	public async Task UpdateTeamName(Guid sessionId, Guid teamId, string name)
	{
		await gameSessionService.UpdateTeamName(sessionId, teamId, name);
	}
}