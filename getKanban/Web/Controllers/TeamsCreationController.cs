using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

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