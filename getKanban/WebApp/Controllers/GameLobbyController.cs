using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class GameLobbyController : Controller
{

	[HttpGet]
	[Route("lobby")]
	public IActionResult Lobby(Guid id)
	{
		//var teamsData = ...;
		return View(id);
	}
}