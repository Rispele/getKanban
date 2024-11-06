using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class TeamsCreationController : Controller
{
	public TeamsCreationController()
	{
		
	}
	
	public IActionResult Menu()
	{
		return View();
	}
}