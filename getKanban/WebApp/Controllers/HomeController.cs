using Core.RequestContexts;
using Core.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class HomeController : Controller
{
	private readonly IUserService userService;

	public HomeController(IUserService userService)
	{
		this.userService = userService;
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
			var user = await userService.GetUserById(requestContext!.GetUserId());
			return View((object)user!.Name);
		}
		catch (InvalidOperationException e)
		{
			var recreatedRequestContext = await ProvideNewUser(new RequestContext());
			var user = await userService.GetUserById(recreatedRequestContext!.GetUserId());
			return View((object)user!.Name);
		}
	}

	private async Task<RequestContext> ProvideNewUser(RequestContext? requestContext)
	{
		var userId = await userService.CreateNewUser("Аноним");

		requestContext = new RequestContext();
		requestContext.AddHeader(RequestContextKeys.UserId, userId.ToString());
			
		Response.Cookies.Append(RequestContextKeys.UserId, userId.ToString());
		return requestContext;
	}

	[HttpGet("add-username")]
	public async Task AddUserName(string name)
	{
		var userId = Guid.Parse(Request.Cookies[RequestContextKeys.UserId] ?? throw new InvalidOperationException());
		await userService.SetUserName(userId, name);
	}
}