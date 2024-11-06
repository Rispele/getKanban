using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("join")]
public class JoinGameController : Controller
{
	[HttpGet]
	[Route("game")]
	public IActionResult JoinGame()
	{
		return View();
	}

	[HttpPost]
	[Route("game")]
	public bool CheckForOpenedGame(Guid id)
	{
		return true;
	}
}