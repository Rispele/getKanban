using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

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