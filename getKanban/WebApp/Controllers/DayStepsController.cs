﻿using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("step")]
public class DayStepsController : Controller
{
	private readonly IGameSessionService gameSessionService;
	private readonly ITeamService teamService;
	
	public DayStepsController(IGameSessionService gameSessionService, ITeamService teamService)
	{
		this.gameSessionService = gameSessionService;
		this.teamService = teamService;
	}
	
	[HttpGet]
	[Route("1")]
	[Route("1/0")]
	public async Task<IActionResult> Step1Stage0()
	{
		return View(await gameSessionService.GetCurrentSessionId(RequestContextFactory.Build(Request)));
	}
	
	[HttpGet]
	[Route("1/1")]
	public IActionResult Step1Stage1()
	{
		return View();
	}
	
	[HttpGet]
	[Route("1/2")]
	public IActionResult Step1Stage2()
	{
		return View();
	}

	[HttpPost]
	[Route("save-roles-transformation")]
	public async Task SaveRolesTransformation([FromBody] string[][] transformations)
	{
		var currentSessionId = await gameSessionService.GetCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		
		foreach (var transformation in transformations)
		{
			var roleFrom = Enum.Parse<TeamRole>(transformation[0]);
			var roleTo = Enum.Parse<TeamRole>(transformation[1]);
			
			await teamService.PatchDayAsync(
				currentSessionId!.Value,
				currentTeam!.Id,
				currentUser.Id,
				new UpdateTeamRolesCommand
				{
					From = roleFrom,
					To = roleTo,
					Remove = false
				});
		}
	}
	
	[HttpGet]
	[Route("2")]
	[Route("2/0")]
	public async Task<IActionResult> Step2Stage0()
	{
		var currentSessionId = await gameSessionService.GetCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		
		var diceRollResult = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View(diceRollResult.RollDiceContainer);
	}

	[HttpGet]
	[Route("roll")]
	public async Task RollDices()
	{
		var currentSessionId = await gameSessionService.GetCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);

		await teamService.PatchDayAsync(
			currentSessionId!.Value,
			currentTeam!.Id,
			currentUser.Id,
			new RollDiceCommand()
		);
	}
	
	[HttpGet]
	[Route("3")]
	[Route("3/0")]
	public IActionResult Step3Stage0()
	{
		return View();
	}
	
	[HttpGet]
	[Route("4")]
	[Route("4/0")]
	public IActionResult Step4Stage0()
	{
		return View();
	}
	
	[HttpGet]
	[Route("4/1")]
	public IActionResult Step4Stage1()
	{
		return View();
	}
	
	[HttpGet]
	[Route("4/2")]
	public IActionResult Step4Stage2()
	{
		return View();
	}
	
	[HttpGet]
	[Route("5")]
	[Route("5/0")]
	public IActionResult Step5Stage0()
	{
		return View();
	}
	
	[HttpGet]
	[Route("5/1")]
	public IActionResult Step5Stage1()
	{
		return View();
	}
	
	[HttpGet]
	[Route("5/2")]
	public IActionResult Step5Stage2()
	{
		return View();
	}
}