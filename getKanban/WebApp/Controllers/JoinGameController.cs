using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("game")]
public class JoinGameController : Controller
{
	private readonly ISessionService sessionService;
	private readonly IUserService userService;
	
	public JoinGameController(ISessionService sessionService, IUserService userService)
	{
		this.sessionService = sessionService;
		this.userService = userService;
	}
	
	[HttpGet]
	[Route("join")]
	public IActionResult JoinTeamSession()
	{
		return View();
	}

	[HttpGet]
	[Route("check")]
	public async Task<bool> CheckForOpenedGame(Guid sessionId)
	{
		return await sessionService.SessionExist(sessionId);
	}
}