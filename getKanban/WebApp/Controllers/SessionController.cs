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
	public async Task<IActionResult> EditSession(Guid sessionId, Guid teamId)
	{
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
	public async Task<bool> CheckForOpenedGame(Guid sessionId, Guid teamId)
	{
		return await gameSessionService.FindGameSession(RequestContextFactory.Build(Request), sessionId, teamId) != null;
	}
}