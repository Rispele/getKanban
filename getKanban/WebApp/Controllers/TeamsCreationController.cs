using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("game")]
public class TeamsCreationController : Controller
{
	[Route("create")]
	public IActionResult Menu()
	{
		return View();
	}
}