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
	private readonly IStatisticsService statisticsService;

	public AdminController(IGameSessionService gameSessionService, DomainContext context, IStatisticsService statisticsService)
	{
		this.gameSessionService = gameSessionService;
		this.context = context;
		this.statisticsService = statisticsService;
	}
	
	[HttpGet("")]
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

	[HttpGet("days")]
	public async Task<IActionResult> AdminPanelDays(Guid sessionId, Guid teamId)
	{
		var session = await context.FindGameSessionsAsync(sessionId);
		if (session is null)
		{
			return View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
		}

		var team = await context.FindTeamAsync(session.Id, teamId);
		if (team is null)
		{
			return View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
		}

		var currentDay = team.CurrentDay;

		var teamStatistic = await statisticsService.CollectStatistic(session.Id, teamId);
		
		return View(new AdminPanelDaysDto
		{
			SessionId = session.Id,
			TeamId = team.Id,
			TeamName = session.Teams.SingleOrDefault(x => x.Id == teamId)?.Name!,
			CurrentDay = new DayDtoConverter().Convert(team, currentDay),
			StartDayNumber = 9,
			FinishDayNumber = 18,
			TeamStatistic = teamStatistic,
			IsFinished = team.IsTeamSessionEnded
		});
	}

	[HttpGet("edit")]
	public async Task<IActionResult> AdminPanelEditing(Guid sessionId, Guid teamId)
	{
		var session = await context.FindGameSessionsAsync(sessionId);
		if (session is null)
		{
			return View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
		}

		var team = await context.FindTeamAsync(session.Id, teamId);
		if (team is null)
		{
			return View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
		}
		
		return View(new AdminPanelDayEditDto()
		{
			TeamName = session.Teams.SingleOrDefault(x => x.Id == teamId)?.Name!,
			DayNumber = team.CurrentDay.Number,
			SubmittedTickets = team.CurrentDay.ReleaseTicketContainer.TicketIds.ToList(),
			TakenTickets = team.CurrentDay.UpdateSprintBacklogContainer.TicketIds.ToList()
		});
	}
}