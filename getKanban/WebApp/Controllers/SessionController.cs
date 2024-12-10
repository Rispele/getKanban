using Core.Helpers;
using Core.RequestContexts;
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
	
	[HttpGet("join-menu")]
	public IActionResult JoinSessionMenu() => View();

	[HttpGet("create-menu")]
	public IActionResult CreateSessionMenu() => View();

	//TODO: Будем использовать для переподключения и резолва текущего состояния игры
	[HttpGet("{sessionId:guid}")]
	public async Task<IActionResult> GetSession(Guid sessionId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentTeam = await gameSessionService.GetCurrentTeam(requestContext, sessionId);
		var session = await gameSessionService.FindGameSession(requestContext,
			InviteCodeHelper.ConcatInviteCode(sessionId, currentTeam.Id), ignorePermissions: false);
	
		return session is { IsRecruitmentFinished: false }
			? RedirectToAction("LobbyMenu", new { sessionId = session.Id, teamId = currentTeam.Id })
			: View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
	}

	[HttpGet("join")]
	public async Task<Guid?> JoinLobbyMenu(string inviteCode)
	{
		var requestContext = RequestContextFactory.Build(Request);
		
		var session = await gameSessionService.FindGameSession(
			requestContext,
			inviteCode,
			ignorePermissions: false);
		if (session is null)
		{
			return null;
		}
		
		var addParticipantResult = await gameSessionService.AddParticipantAsync(requestContext, inviteCode);
		if (addParticipantResult is null)
		{
			return null;
		}

		return addParticipantResult.GameSession.Id;
	}

	[HttpGet("{sessionId:guid}/{teamId:guid}/lobby-menu")]
	public async Task<IActionResult> LobbyMenu(Guid sessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var session = await gameSessionService.FindGameSession(requestContext,
			InviteCodeHelper.ConcatInviteCode(sessionId, teamId), ignorePermissions: false);
		if (session is null || session.IsRecruitmentFinished)
		{
			return View("Error", "Невозможно подключиться к сессии");
		}
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		if (session.Angels.Participants.Users.All(x => x.Id != currentUser.Id)
		    && session.Teams.All(x => x.Participants.Users.All(p => p.Id != currentUser.Id)))
		{
			await JoinLobbyMenu(InviteCodeHelper.ConcatInviteCode(sessionId, teamId));
			session = await gameSessionService.FindGameSession(requestContext,
				InviteCodeHelper.ConcatInviteCode(sessionId, teamId), ignorePermissions: false);
		}

		if (session is not null)
		{
			session.RequesterId = currentUser.Id;
			requestContext.AddHeader(RequestContextKeys.SessionId, sessionId.ToString());
			Response.Cookies.Append(RequestContextKeys.SessionId, sessionId.ToString());
		}
		
		return View(session);
	}

	[HttpGet("create-session")]
	public Task<Guid> CreateGameSession(string sessionName, long teamsCount)
	{
		var requestContext = RequestContextFactory.Build(Request);
		return gameSessionService.CreateGameSession(requestContext, sessionName, teamsCount);
	}
}