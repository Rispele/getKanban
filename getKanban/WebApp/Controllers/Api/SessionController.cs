using Core;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("session")]
public class SessionController : Controller
{
	private ISessionService sessionService;
	private IUserService userService;

	public SessionController(ISessionService sessionService, IUserService userService)
	{
		this.sessionService = sessionService;
		this.userService = userService;
	}

	[HttpGet]
	[Route("create")]
	public async Task<Guid?> CreateSession(int teamsCount, string sessionName)
	{
		var sessionId = await sessionService.TryCreateSession(sessionName, teamsCount);
		return sessionId;
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