using Core.RequestContexts;
using Core.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

public class HomeController : Controller
{
	private readonly IUserService userService;
	private readonly IGameSessionService gameSessionService;

	public HomeController(IUserService userService, IGameSessionService gameSessionService)
	{
		this.userService = userService;
		this.gameSessionService = gameSessionService;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		if (!RequestContextFactory.TryBuild(ControllerContext.HttpContext.Request, out var requestContext))
		{
			requestContext = await ProvideNewUser(requestContext);
		}

		try
		{
			return await BuildViewModel(requestContext);
		}
		catch (InvalidOperationException e)
		{
			var recreatedRequestContext = await ProvideNewUser(new RequestContext());
			return await BuildViewModel(recreatedRequestContext);
		}
	}

	[HttpGet("add-username")]
	public async Task AddUserName(string name)
	{
		var userId = Guid.Parse(Request.Cookies[RequestContextKeys.UserId] ?? throw new InvalidOperationException());
		await userService.SetUserName(userId, name);
	}

	private async Task<IActionResult> BuildViewModel(RequestContext? requestContext)
	{
		var user = await userService.GetUserById(requestContext!.GetUserId());
		var userRelatedSessionDtos = await gameSessionService.GetUserRelatedSessions(user.Id);
		var model = new MainMenuModel
		{
			UserName = user.Name,
			UserGames = userRelatedSessionDtos.Select(
				x => new MainMenuGameModel
				{
					GameSessionId = x.GameSessionId,
					TeamId = x.TeamId,
					GameSessionName = x.GameSessionName,
					GameSessionStatus = x.GameSessionStatus,
					ParticipantRole = x.RequesterParticipantRole,
					TeamsCount = x.TeamsCount
				}).ToList()
		};
		return View(model);
	}

	private async Task<RequestContext> ProvideNewUser(RequestContext? requestContext)
	{
		var userId = await userService.CreateNewUser("Аноним");

		requestContext = new RequestContext();
		requestContext.AddHeader(RequestContextKeys.UserId, userId.ToString());

		Response.Cookies.Append(RequestContextKeys.UserId, userId.ToString());
		return requestContext;
	}
}