using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers.TeamMembers;
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
	public async Task<IActionResult> Step1Stage2()
	{
		var currentSessionId = await gameSessionService.GetCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);

		var teamMembers = (await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id))
			.TeamMembersContainer.TeamRoleUpdates.ToList();
		
		return View(teamMembers);
	}

	[HttpPost]
	[Route("save-roles-transformation")]
	public async Task SaveRolesTransformation([FromBody] string[] transformation)
	{
		var currentSessionId = await gameSessionService.GetCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		
		var teamMemberId = long.Parse(transformation[0]);
		var roleTo = Enum.Parse<TeamRole>(transformation[1]);
		
		await teamService.PatchDayAsync(
			currentSessionId!.Value,
			currentTeam!.Id,
			currentUser.Id,
			new UpdateTeamRolesCommand
			{
				TeamMemberId = teamMemberId,
				To = roleTo
			});
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

	public class TicketModel
	{
		public string TicketId { get; set; }
		public bool Remove { get; set; }
	}

	[HttpPost]
	[Route("update-sprint-backlog")]
	public async Task UpdateSprintBacklog([FromBody] TicketModel ticketModel)
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
			new UpdateSprintBacklogCommand()
			{
				TicketIds = new[] { ticketModel.TicketId },
				Remove = ticketModel.Remove
			}
		);
	}

	[HttpGet]
	[Route("4/2")]
	public async Task<IActionResult> Step4Stage2()
	{
		var currentSessionId = await gameSessionService.GetCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		var ticketIds = (await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id))
			.UpdateSprintBacklogContainer.TicketIds.ToList();
		
		return View(ticketIds);
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