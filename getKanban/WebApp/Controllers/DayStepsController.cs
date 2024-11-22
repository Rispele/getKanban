using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("step")]
public class DayStepsController : Controller
{
	[HttpGet]
	[Route("1")]
	[Route("1/0")]
	public IActionResult Step1Stage0()
	{
		return View();
	}
	
	[HttpGet]
	[Route("1/1")]
	public IActionResult Step1Stage1()
	{
		return View();
	}
	
	[HttpGet]
	[Route("1/2")]
	public IActionResult Step1Stage2()
	{
		return View();
	}
	
	[HttpGet]
	[Route("2")]
	[Route("2/0")]
	public IActionResult Step2Stage0()
	{
		return View();
	}
	
	[HttpGet]
	[Route("3")]
	[Route("3/0")]
	public IActionResult Step3Stage0()
	{
		return View();
	}
	
	[HttpGet]
	[Route("4")]
	[Route("4/0")]
	public IActionResult Step4Stage0()
	{
		return View();
	}
	
	[HttpGet]
	[Route("4/1")]
	public IActionResult Step4Stage1()
	{
		return View();
	}
	
	[HttpGet]
	[Route("4/2")]
	public IActionResult Step4Stage2()
	{
		return View();
	}
	
	[HttpGet]
	[Route("5")]
	[Route("5/0")]
	public IActionResult Step5Stage0()
	{
		return View();
	}
	
	[HttpGet]
	[Route("5/1")]
	public IActionResult Step5Stage1()
	{
		return View();
	}
	
	[HttpGet]
	[Route("5/2")]
	public IActionResult Step5Stage2()
	{
		return View();
	}
}