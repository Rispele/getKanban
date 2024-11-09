using Core.RequestContexts;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("session")]
public class SessionController : Controller
{
	private IGameSessionService gameSessionService;
	private IUserService userService;

	public SessionController(IGameSessionService gameSessionService, IUserService userService)
	{
		this.gameSessionService = gameSessionService;
		this.userService = userService;
	}

	[HttpGet]
	[Route("create")]
	public async Task<Guid?> CreateSession(int teamsCount, string sessionName)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var session = await gameSessionService.CreateGameSession(requestContext, sessionName, teamsCount);
		return session.Id;
	}

	[HttpGet]
	[Route("user/create")]
	public async Task<Guid> CreateUser(string name)
	{
		var newUserId = await userService.CreateNewUser(name);
		HttpContext.Response.Cookies.Append("userId", newUserId.ToString());
		return newUserId;
	}
}