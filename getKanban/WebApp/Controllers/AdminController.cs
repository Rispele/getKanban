using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.Dtos.Converters;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Game;
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
		if (session is null)
		{
			return View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
		}
		return View(new AdminPanelTeamsDto()
		{
			GameSessionId = session.Id,
			GameSessionName = session.Name,
			Teams = session.Teams.Select(x => TeamDtoConverter.For(ParticipantRole.Angel).Convert(x)!).ToList()
		});
	}

	[HttpGet]
	[Route("days")]
	public async Task<IActionResult> AdminPanelDays(Guid sessionId, Guid teamId)
	{
		var session = await context.FindGameSessionsAsync(sessionId);
		if (session is null)
		{
			return View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
		}
		
		return View(new AdminPanelDaysDto
		{
			TeamName = session.Teams.SingleOrDefault(x => x.Id == teamId)?.Name!,
			Days = [new(), new(), new()]
		});
	}

	[HttpGet]
	[Route("edit")]
	public IActionResult AdminPanelEditing(Guid sessionId, Guid teamId, int dayNumber)
	{
		//var dayConfiguration = ;
		return View();
	}
}