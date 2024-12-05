using Core.Dtos;
using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
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

	[HttpPost("another-team-roll")]
	public async Task AnotherTeamRoll()
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
			new WorkAnotherTeamDayCommand());
	}
	

	[HttpGet("1")]
	[HttpGet("1/0")]
	public async Task<IActionResult> Step1Stage0()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);

		var currentDay = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		var shouldRollForAnotherTeam = currentDay
			.AwaitedCommands.Any(x => x is { CommandType: DayCommandType.WorkAnotherTeam});

		if (shouldRollForAnotherTeam)
		{
			return View("AnotherTeamRoll", (currentTeam.Id, currentDay.Number));
		}
		
		return View((await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)), currentDay.Number));
	}

	[HttpGet("1/1")]
	public async Task<IActionResult> Step1Stage1()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		var currentDay = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View((await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)), currentDay.Number));
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

		var currentDay = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		var diceRollResult = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View((currentSessionId, currentDay.Number, diceRollResult.RollDiceContainer));
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
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		var currentDay = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View((await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)), currentDay.Number));
	}

	[HttpGet("4")]
	[HttpGet("4/0")]
	public async Task<IActionResult> Step4Stage0()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		var currentDay = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View((await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)), currentDay.Number));
	}

	[HttpGet("4/1")]
	public async Task<IActionResult> Step4Stage1()
	{
		var sessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			sessionId!.Value);

		var currentDay = await teamService.GetCurrentDayAsync(sessionId!.Value, currentTeam!.Id);
		var shouldShowTickets = currentDay
			.AwaitedCommands.Any(x => x is { CommandType: DayCommandType.ReleaseTickets });

		if (!shouldShowTickets)
		{
			return View("Step5Stage0", (sessionId, currentDay.Number));
		}
		
		var ticketIds = await gameSessionService.GetReleaseTickets(sessionId!.Value, currentTeam.Id);
		var pageTypeNumber = (ticketIds.Any(x => x.Contains('S')) ? 1 : 0) +
		               (ticketIds.Any(x => x.Contains('I')) ? 1 : 0) +
		               (ticketIds.Any(x => x.Contains('E') || x.Contains('F')) ? 1 : 0);
		var pageType = pageTypeNumber switch
		{
			1 => "Single",
			2 => "Double",
			3 => "Triple",
			_ => null
		};

		return View(new TicketsViewDto
			{
				SessionId = sessionId!.Value,
				PageType = pageType!,
				TicketIds = ticketIds
			});
	}

	public class TicketModel
	{
		public string TicketId { get; set; }
		public bool Remove { get; set; }
	}

	[HttpPost("update-release")]
	public async Task UpdateRelease([FromBody] TicketModel ticketModel)
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
			new ReleaseTicketsCommand()
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
			.ReleaseTicketContainer.TicketIds.ToList();
		
		return View((currentSessionId, ticketIds));
	}

	[HttpGet("5")]
	[HttpGet("5/0")]
	public async Task<IActionResult> Step5Stage0()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		var currentDay = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View((await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)), currentDay.Number));
	}

	[HttpGet("5/1")]
	public async Task<IActionResult> Step5Stage1()
	{
		var sessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			sessionId!.Value);
		
		var shouldShowTickets = (await teamService.GetCurrentDayAsync(sessionId!.Value, currentTeam!.Id))
			.AwaitedCommands.Any(x => x is { CommandType: DayCommandType.UpdateSprintBacklog });

		if (!shouldShowTickets)
		{
			return View("Step6Stage0", sessionId!.Value);
		}
		
		var ticketIds = await gameSessionService.GetBacklogTickets(sessionId!.Value, currentTeam.Id);
		var pageTypeNumber = (ticketIds.Any(x => x.Contains('S')) ? 1 : 0) +
		                     (ticketIds.Any(x => x.Contains('I')) ? 1 : 0) +
		                     (ticketIds.Any(x => x.Contains('E') || x.Contains('F')) ? 1 : 0);
		var pageType = pageTypeNumber switch
		{
			1 => "Single",
			2 => "Double",
			3 => "Triple",
			_ => null
		};

		return View(new TicketsViewDto
		{
			SessionId = sessionId!.Value,
			PageType = pageType!,
			TicketIds = ticketIds
		});
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

	[HttpGet("5/2")]
	public async Task<IActionResult> Step5Stage2()
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
	
	[HttpGet("6")]
	[HttpGet("6/0")]
	public async Task<IActionResult> Step6Stage0()
	{
		return View(await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)));
	}

	[HttpPost("update-cfd")]
	public async Task UpdateCfd([FromBody] CfdDayDataModel cfdDayDataModel)
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);

		await PatchDayAsync(UpdateCfdContainerPatchType.Released, cfdDayDataModel.Released);
		await PatchDayAsync(UpdateCfdContainerPatchType.ToDeploy, cfdDayDataModel.ToDeploy);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithTesters, cfdDayDataModel.WithTesters);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithProgrammers, cfdDayDataModel.WithProgrammers);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithAnalysts, cfdDayDataModel.WithAnalysts);
		return;


		async Task PatchDayAsync(UpdateCfdContainerPatchType patchType, int value)
		{
			await teamService.PatchDayAsync(
				currentSessionId!.Value,
				currentTeam!.Id,
				currentUser.Id,
				new UpdateCfdCommand()
				{
					PatchType = patchType,
					Value = value
				}
			);
		}
	}
	
	public class CfdDayDataModel 
	{
		public int Released { get; set; }
		public int ToDeploy { get; set; }
		public int WithTesters { get; set; }
		public int WithProgrammers { get; set; }
		public int WithAnalysts { get; set; }
	}
	
	
	[HttpGet("6/1")]
	public async Task<IActionResult> Step6Stage1()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);

		var cfd = await gameSessionService.GetCfdDataForTeam(currentSessionId!.Value, currentTeam.Id);
		cfd.SessionId = currentSessionId!.Value;
		
		return View(cfd);
	}

	[HttpGet("7")]
	[HttpGet("7/0")]
	public async Task<IActionResult> Step7Stage0()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		var currentDay = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View((await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)), currentDay.Number));
	}

	[HttpGet("7/1")]
	public async Task<IActionResult> Step7Stage1()
	{
		var currentSessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		var currentUser = await gameSessionService.GetCurrentUser(RequestContextFactory.Build(Request));
		var currentTeam = await gameSessionService.GetCurrentTeam(
			RequestContextFactory.Build(Request),
			currentSessionId!.Value);
		var currentDay = await teamService.GetCurrentDayAsync(currentSessionId!.Value, currentTeam!.Id);
		return View((await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request)), currentDay.Number));
	}

	[HttpPost("end-day")]
	public async Task EndDay()
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
			new EndDayCommand()
		);
	}
}