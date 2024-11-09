using Core;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Route("lobby")]
public class CommandEditorController : Controller
{
	private readonly ISessionService sessionService;

	public CommandEditorController(ISessionService sessionService)
	{
		this.sessionService = sessionService;
	}
	
	[HttpGet]
	[Route("")]
	public async Task<IActionResult> CommandEditor(Guid sessionId)
	{
		var (teams, sessionName) = await sessionService.TryGetSession(sessionId);
		return View(new LobbyViewModel
		{
			GameTitle = sessionName,
			Teams = teams.OrderBy(x => x.Name).ToList()
		});
	}
}