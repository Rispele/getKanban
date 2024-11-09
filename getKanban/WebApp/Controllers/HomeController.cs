﻿using Core.RequestContexts;
using Core.Services;
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
}