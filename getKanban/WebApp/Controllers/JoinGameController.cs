using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("game")]
public class JoinGameController : Controller
{
	private readonly IGameSessionService gameSessionService;
	private readonly IUserService userService;
	
	public JoinGameController(IGameSessionService gameSessionService, IUserService userService)
	{
		this.gameSessionService = gameSessionService;
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
		return await gameSessionService.FindGameSession(sessionId) != null;
	}
}