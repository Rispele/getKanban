using Core.RequestContexts;
using Core.Services.Contracts;
using Microsoft.AspNetCore.Http.Headers;
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
			var userId = await userService.CreateNewUser("Аноним");

			requestContext = new RequestContext();
			requestContext.AddHeader(RequestContextKeys.UserId, userId.ToString());
			
			Response.Cookies.Append(RequestContextKeys.UserId, userId.ToString());
		}

		var user = await userService.GetUserById(requestContext!.GetUserId());
		return View((object)user!.Name);
	}

	[HttpGet("add-username")]
	public async Task AddUserName(string name)
	{
		var userId = Guid.Parse(Request.Cookies[RequestContextKeys.UserId] ?? throw new InvalidOperationException());
		await userService.SetUserName(userId, name);
	}
}