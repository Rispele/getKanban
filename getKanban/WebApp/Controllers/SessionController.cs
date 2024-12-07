using Core.Dtos;
using Core.Helpers;
using Core.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;

namespace WebApp.Controllers;

[Route("session")]
public class SessionController : Controller
{
	private readonly IGameSessionService gameSessionService;
	private readonly IHubContext<LobbyHub> hub;

	public SessionController(IGameSessionService gameSessionService, IHubContext<LobbyHub> hub)
	{
		this.gameSessionService = gameSessionService;
		this.hub = hub;
	}

	//TODO: Будем использовать для переподключения и резолва текущего состояния игры
	[HttpGet("{sessionId:guid}")]
	public async Task<IActionResult> GetSession(Guid sessionId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var session = await gameSessionService.FindGameSession(requestContext, sessionId, ignorePermissions: false);
		var currentTeam = await gameSessionService.GetCurrentTeam(requestContext, sessionId);

		return session is { IsRecruitmentFinished: false }
			? RedirectToAction("LobbyMenu", new { sessionId = session.Id, teamId = currentTeam.Id })
			: View("Error", "Запрашиваемая сессия не была найдена или закрыта.");
	}

	[HttpGet("join")]
	public async Task<Guid?> JoinLobbyMenu(string inviteCode)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var addParticipantResult = await gameSessionService.AddParticipantAsync(requestContext, inviteCode);
		if (addParticipantResult is null)
		{
			return null;
		}
		
		// if (addParticipantResult.Updated)
		// {
		// 	await hub.Clients.Group($"lobby-{addParticipantResult.GameSession.Id}").SendAsync(
		// 		"NotifyJoined",
		// 		addParticipantResult.UpdatedTeamId.ToString(),
		// 		addParticipantResult.User.Id.ToString(),
		// 		addParticipantResult.User.Name);
		// }

		return addParticipantResult.GameSession.Id;
	}

	[HttpGet("leave")]
	public async Task<bool> LeaveLobbyMenu(Guid sessionId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var user = await gameSessionService.GetCurrentUser(requestContext);
		return await gameSessionService.RemoveParticipantAsync(requestContext, sessionId, user.Id);
	}

	[HttpGet("join-menu")]
	public IActionResult JoinSessionMenu() => View();

	[HttpGet("create-menu")]
	public IActionResult CreateSessionMenu() => View();

	[HttpGet("{sessionId:guid}/{teamId:guid}/lobby-menu")]
	public async Task<IActionResult> LobbyMenu(Guid sessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var session = await gameSessionService.FindGameSession(requestContext,
			InviteCodeHelper.ConcatInviteCode(sessionId, teamId), ignorePermissions: false);
		if (session is null)
		{
			return View("Error", "Невозможно подключиться к сессии");
		}
		var currentUser = await gameSessionService.GetCurrentUser(requestContext);
		if (session!.Angels.Participants.Users.All(x => x.Id != currentUser.Id)
		    && session.Teams.All(x => x.Participants.Users.All(p => p.Id != currentUser.Id)))
		{
			await JoinLobbyMenu(InviteCodeHelper.ConcatInviteCode(sessionId, teamId));
			session = await gameSessionService.FindGameSession(requestContext,
				InviteCodeHelper.ConcatInviteCode(sessionId, teamId), ignorePermissions: false);
		}
		
		return View(session);
	}
	
	[HttpGet("check")]
	public async Task<bool> CheckForOpenedGame(string inviteCode)
	{
		var session = await gameSessionService.FindGameSession(
			RequestContextFactory.Build(Request),
			inviteCode,
			ignorePermissions: false);
		return session is not null;
	}

	[HttpGet("create-session")]
	public Task<Guid> CreateGameSession(string sessionName, long teamsCount)
	{
		var requestContext = RequestContextFactory.Build(Request);
		return gameSessionService.CreateGameSession(requestContext, sessionName, teamsCount);
	}

	[HttpGet("get-current-team")]
	public async Task<TeamDto?> GetCurrentTeam(Guid sessionId)
	{
		try
		{
			return await gameSessionService.GetCurrentTeam(RequestContextFactory.Build(Request), sessionId);
		}
		catch (InvalidOperationException e)
		{
			return null;
		}
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