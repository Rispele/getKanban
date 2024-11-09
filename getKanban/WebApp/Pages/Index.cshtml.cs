using Core.RequestContexts;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
	private IUserService userService;

	public IndexModel(IUserService userService)
	{
		this.userService = userService;
	}

	public async Task OnGet()
	{
		if (!RequestContextFactory.TryBuild(HttpContext.Request, out _))
		{
			var user = await userService.CreateNewUser(Guid.NewGuid().ToString());
			Response.Cookies.Append(RequestContextKeys.UserId, user.ToString());
		}
	}
}