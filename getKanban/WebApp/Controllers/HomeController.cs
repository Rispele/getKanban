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

	public async Task<IActionResult> Index()
	{
		if (!RequestContextFactory.TryBuild(ControllerContext.HttpContext.Request, out _))
		{
			var user = await userService.CreateNewUser(Guid.NewGuid().ToString());
			Response.Cookies.Append(RequestContextKeys.UserId, user.ToString());
		}
		
		return View();
	}

	[HttpGet("add-username")]
	public async Task AddUserName(string name)
	{
		var userId = Guid.Parse(Request.Cookies[RequestContextKeys.UserId] ?? throw new InvalidOperationException());
		var user = await userService.GetUserById(userId) ?? throw new InvalidOperationException();
		await userService.SetUserName(user.Id, name);
	}
}