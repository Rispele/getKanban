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

	[HttpGet("")]
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

	[HttpGet("join")]
	public IActionResult JoinSession() => View();

	[HttpGet("create")]
	public IActionResult CreateSession() => View();

	[HttpGet("check")]
	public async Task<bool> CheckForOpenedGame(string invite)
	{
		var session = await gameSessionService.FindGameSession(
			RequestContextFactory.Build(Request),
			invite,
			ignorePermissions: false);
		return session is not null;
	}

	[HttpGet("create-session")]
	public async Task<Guid?> CreateGameSession(string sessionName, long teamsCount)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var sessionId = await gameSessionService.CreateGameSession(requestContext, sessionName, teamsCount);
		return sessionId;
	}

	[HttpGet("get-current-team")]
	public async Task<TeamDto?> GetCurrentTeam(Guid sessionId)
	{
		return await gameSessionService.GetCurrentTeam(RequestContextFactory.Build(Request), sessionId);
	}
	
	[HttpGet("get-team-invite")]
	public Guid GetTeamInviteId(string invite)
	{
		return gameSessionService.GetTeamInviteId(invite);
	}
	
	[HttpGet("update-team-name")]
	public async Task UpdateTeamName(Guid sessionId, Guid teamId, string name)
	{
		await gameSessionService.UpdateTeamName(sessionId, teamId, name);
	}
}