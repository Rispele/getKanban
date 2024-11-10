using Core.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

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
		var (sessionId, teamId) = InviteCodeHelper.SplitInviteCode(invite);
		var session = await gameSessionService.FindGameSession(RequestContextFactory.Build(Request), sessionId, teamId);
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
		var (sessionId, teamId) = InviteCodeHelper.SplitInviteCode(invite);
		return await gameSessionService.FindGameSession(RequestContextFactory.Build(Request), sessionId, teamId) != null;
	}

	[HttpGet]
	[Route("get-current-team")]
	public async Task<Guid?> GetCurrentTeam(Guid sessionId)
	{
		return await gameSessionService.GetCurrentTeam(RequestContextFactory.Build(Request), sessionId);
	}
	
	[HttpGet]
	[Route("get-team-invite")]
	public Guid GetTeamInviteId(string invite)
	{
		return gameSessionService.GetTeamInviteId(invite);
	}
}