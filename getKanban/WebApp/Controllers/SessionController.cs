using Core.Dtos;
using Core.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("session")]
public class SessionController : Controller
{
	private readonly IGameSessionService gameSessionService;

	public SessionController(IGameSessionService gameSessionService)
	{
		this.gameSessionService = gameSessionService;
	}

	[HttpGet]
	[Route("")]
	public async Task<IActionResult> EditSession(string invite)
	{
		var session = await gameSessionService.FindGameSession(
			RequestContextFactory.Build(Request),
			invite,
			ignorePermissions: false);
		return session is { IsRecruitmentFinished: false }
			? View(session)
			: View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
	}

	[HttpGet]
	[Route("join")]
	public IActionResult JoinSession()
	{
		return View();
	}

	[HttpGet]
	[Route("create")]
	public IActionResult CreateSession()
	{
		return View();
	}

	[HttpGet]
	[Route("check")]
	public async Task<bool> CheckForOpenedGame(string invite)
	{
		var session = await gameSessionService.FindGameSession(
			RequestContextFactory.Build(Request),
			invite,
			ignorePermissions: false);
		return session is not null;
	}

	[HttpGet]
	[Route("get-current-team")]
	public async Task<TeamDto?> GetCurrentTeam(Guid sessionId)
	{
		return await gameSessionService.GetCurrentTeam(RequestContextFactory.Build(Request), sessionId);
	}
	
	[HttpGet]
	[Route("get-team-invite")]
	public Guid GetTeamInviteId(string invite)
	{
		return gameSessionService.GetTeamInviteId(invite);
	}
	
	[HttpGet]
	[Route("update-team-name")]
	public async Task UpdateTeamName(Guid sessionId, Guid teamId, string name)
	{
		await gameSessionService.UpdateTeamName(sessionId, teamId, name);
	}
}