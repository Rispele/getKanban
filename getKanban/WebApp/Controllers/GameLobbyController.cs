using Core.Services;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Route("lobby")]
public class CommandEditorController : Controller
{
	private readonly IGameSessionService gameSessionService;

	public CommandEditorController(IGameSessionService gameSessionService)
	{
		this.gameSessionService = gameSessionService;
	}

	[HttpGet]
	[Route("")]
	public async Task<IActionResult> CommandEditor(Guid sessionId, Guid teamId)
	{
		var session = await gameSessionService.FindGameSession(RequestContextFactory.Build(Request), sessionId, teamId);
		return View(session);
	}
}