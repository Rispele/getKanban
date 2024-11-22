using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("admin")]
public class AdminController : Controller
{
	private readonly IGameSessionService gameSessionService;
	private readonly DomainContext context;

	public AdminController(IGameSessionService gameSessionService, DomainContext context)
	{
		this.gameSessionService = gameSessionService;
		this.context = context;
	}
	
	[HttpGet]
	[Route("")]
	public async Task<IActionResult> AdminPanelStart(Guid sessionId)
	{
		var session = await context.FindGameSessionsAsync(sessionId);
		return View(session?.Teams.ToList());
	}

	[HttpGet]
	[Route("days")]
	public IActionResult AdminPanelDays(Guid sessionId, Guid teamId)
	{
		//var days = ;
		return View();
	}

	[HttpGet]
	[Route("edit")]
	public IActionResult AdminPanelEditing(Guid sessionId, Guid teamId, int dayNumber)
	{
		//var dayConfiguration = ;
		return View();
	}
}