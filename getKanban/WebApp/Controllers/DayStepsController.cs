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

	[HttpGet("1")]
	[HttpGet("1/0")]
	public async Task<IActionResult> Step1Stage0()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}

	[HttpGet("1/1")]
	public async Task<IActionResult> Step1Stage1()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}

	[HttpGet("1/2")]
	public async Task<IActionResult> Step1Stage2()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);

		var teamMembers = (await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id))
			.TeamMembersContainer.TeamRoleUpdates.ToList();
		
		return View((currentSessionId, teamMembers));
	}

	[HttpPost("save-roles-transformation")]
	public async Task SaveRolesTransformation([FromBody] string[] transformation)
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
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

	[HttpGet("2")]
	[HttpGet("2/0")]
	public async Task<IActionResult> Step2Stage0()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);

		var diceRollResult = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View((currentSessionId, diceRollResult.RollDiceContainer));
	}

	[HttpGet("roll")]
	public async Task RollDices()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
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

	[HttpGet("3")]
	[HttpGet("3/0")]
	public async Task<IActionResult> Step3Stage0()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}

	[HttpGet("4")]
	[HttpGet("4/0")]
	public async Task<IActionResult> Step4Stage0()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}

	[HttpGet("4/1")]
	public async Task<IActionResult> Step4Stage1()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}

	public class TicketModel
	{
		public string TicketId { get; set; }
		public bool Remove { get; set; }
	}

	[HttpPost("update-sprint-backlog")]
	public async Task UpdateSprintBacklog([FromBody] TicketModel ticketModel)
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
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

	[HttpGet("4/2")]
	public async Task<IActionResult> Step4Stage2()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		var ticketIds = (await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id))
			.UpdateSprintBacklogContainer.TicketIds.ToList();
		
		return View((currentSessionId, ticketIds));
	}

	[HttpGet("5")]
	[HttpGet("5/0")]
	public async Task<IActionResult> Step5Stage0()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}

	[HttpGet("5/1")]
	public async Task<IActionResult> Step5Stage1()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}

	[HttpGet("5/2")]
	public async Task<IActionResult> Step5Stage2()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}
}