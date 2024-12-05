﻿using Core.Dtos;
using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.TeamMembers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("{gameSessionId:guid}/{teamId:guid}/step")]
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
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		await teamService.PatchDayAsync(credentials, new WorkAnotherTeamDayCommand());
	}


	[HttpGet("1")]
	[HttpGet("1/0")]
	public async Task<IActionResult> Step1Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

		var shouldRollForAnotherTeam = currentDay.AwaitedCommands
			.Any(x => x is { CommandType: DayCommandType.WorkAnotherTeam });

		return shouldRollForAnotherTeam
			? View("AnotherTeamRoll", currentDay.FullId)
			: View(currentDay.FullId);
	}

	[HttpGet("1/1")]
	public async Task<IActionResult> Step1Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		return View(currentDay.FullId);
	}

	[HttpGet("1/2")]
	public async Task<IActionResult> Step1Stage2(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		var teamMembers = currentDay.TeamMembersContainer.TeamRoleUpdates.ToList();
		return View((currentDay.FullId, teamMembers));
	}

	[HttpPost("save-roles-transformation")]
	public async Task SaveRolesTransformation([FromBody] string[] transformation)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));

		var teamMemberId = long.Parse(transformation[0]);
		var roleTo = Enum.Parse<TeamRole>(transformation[1]);

		await teamService.PatchDayAsync(
			credentials,
			new UpdateTeamRolesCommand { TeamMemberId = teamMemberId, To = roleTo });
	}

	[HttpGet("2")]
	[HttpGet("2/0")]
	public async Task<IActionResult> Step2Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId);
		return View((credentials.SessionId, currentDay.Number, currentDay.RollDiceContainer));
	}

	[HttpGet("roll")]
	public async Task RollDices(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		await teamService.PatchDayAsync(credentials, new RollDiceCommand());
	}

	[HttpGet("3")]
	[HttpGet("3/0")]
	public async Task<IActionResult> Step3Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId);
		return View((credentials.SessionId, currentDay.Number));
	}

	[HttpGet("4")]
	[HttpGet("4/0")]
	public async Task<IActionResult> Step4Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId);
		return View((credentials.SessionId, currentDay.Number));
	}

	[HttpGet("4/1")]
	public async Task<IActionResult> Step4Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));

		var currentDay = await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId);
		var shouldShowTickets = currentDay.AwaitedCommands
			.Any(x => x is { CommandType: DayCommandType.ReleaseTickets });

		if (!shouldShowTickets)
		{
			return View("Step5Stage0", (credentials.SessionId, currentDay.Number));
		}

		var ticketIds = await gameSessionService.GetTicketsToRelease(credentials.SessionId, credentials.TeamId);
		var pageTypeNumber = (ticketIds.Any(x => x.id.Contains('S')) ? 1 : 0) +
		                     (ticketIds.Any(x => x.id.Contains('I')) ? 1 : 0) +
		                     (ticketIds.Any(x => x.id.Contains('E') || x.id.Contains('F')) ? 1 : 0);
		var pageType = pageTypeNumber switch
		{
			1 => "Single",
			2 => "Double",
			3 => "Triple",
			_ => null
		};

		return View(
			new TicketsViewDto
			{
				SessionId = credentials.SessionId,
				CurrentDayNumber = currentDay.Number,
				PageType = pageType!,
				TicketIds = ticketIds
			});
	}

	public class TicketModel
	{
		public string TicketId { get; [UsedImplicitly] set; }
		public bool Remove { get; [UsedImplicitly] set; }
	}

	[HttpPost("update-release")]
	public async Task UpdateRelease([FromBody] TicketModel ticketModel, Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));

		await teamService.PatchDayAsync(
			credentials,
			ReleaseTicketsCommand.Create(ticketModel.TicketId, ticketModel.Remove));
	}

	[HttpGet("4/2")]
	public async Task<IActionResult> Step4Stage2(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var ticketIds = (await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId))
			.ReleaseTicketContainer.TicketIds.ToList();
		return View((credentials.SessionId, ticketIds));
	}

	[HttpGet("5")]
	[HttpGet("5/0")]
	public async Task<IActionResult> Step5Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId);
		return View((credentials.SessionId, currentDay.Number));
	}

	[HttpGet("5/1")]
	public async Task<IActionResult> Step5Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));

		var currentDay = await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId);
		var shouldShowTickets =
			currentDay.AwaitedCommands.Any(x => x is { CommandType: DayCommandType.UpdateSprintBacklog });

		if (!shouldShowTickets)
		{
			return View("Step6Stage0", credentials.SessionId);
		}

		var ticketIds = await gameSessionService.GetBacklogTickets(credentials.SessionId, credentials.TeamId);
		var pageTypeNumber = (ticketIds.Any(x => x.id.Contains('S')) ? 1 : 0) +
		                     (ticketIds.Any(x => x.id.Contains('I')) ? 1 : 0) +
		                     (ticketIds.Any(x => x.id.Contains('E') || x.id.Contains('F')) ? 1 : 0);
		var pageType = pageTypeNumber switch
		{
			1 => "Single",
			2 => "Double",
			3 => "Triple",
			_ => null
		};

		return View(
			new TicketsViewDto
			{
				SessionId = credentials.SessionId,
				PageType = pageType!,
				TicketIds = ticketIds,
				CurrentDayNumber = currentDay.Number,
			});
	}

	[HttpPost("update-sprint-backlog")]
	public async Task UpdateSprintBacklog([FromBody] TicketModel ticketModel, Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));

		await teamService.PatchDayAsync(
			credentials,
			new UpdateSprintBacklogCommand()
			{
				TicketIds = new[] { ticketModel.TicketId }, Remove = ticketModel.Remove
			});
	}

	[HttpGet("5/2")]
	public async Task<IActionResult> Step5Stage2(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var ticketIds = (await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId))
			.UpdateSprintBacklogContainer.TicketIds.ToList();
		return View((credentials.SessionId, ticketIds));
	}

	[HttpGet("6")]
	[HttpGet("6/0")]
	public async Task<IActionResult> Step6Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var sessionId = await gameSessionService.FindCurrentSessionId(RequestContextFactory.Build(Request));
		return View(sessionId!.Value);
	}

	[HttpPost("update-cfd")]
	public async Task UpdateCfd([FromBody] CfdDayDataModel cfdDayDataModel, Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));

		await PatchDayAsync(UpdateCfdContainerPatchType.Released, cfdDayDataModel.Released);
		await PatchDayAsync(UpdateCfdContainerPatchType.ToDeploy, cfdDayDataModel.ToDeploy);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithTesters, cfdDayDataModel.WithTesters);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithProgrammers, cfdDayDataModel.WithProgrammers);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithAnalysts, cfdDayDataModel.WithAnalysts);
		return;

		async Task PatchDayAsync(UpdateCfdContainerPatchType patchType, int value)
		{
			await teamService.PatchDayAsync(
				credentials,
				new UpdateCfdCommand()
				{
					PatchType = patchType, Value = value
				});
		}
	}

	public class CfdDayDataModel
	{
		public int Released { get; [UsedImplicitly] set; }
		public int ToDeploy { get; [UsedImplicitly] set; }
		public int WithTesters { get; [UsedImplicitly] set; }
		public int WithProgrammers { get; [UsedImplicitly] set; }
		public int WithAnalysts { get; [UsedImplicitly] set; }
	}


	[HttpGet("6/1")]
	public async Task<IActionResult> Step6Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var cfd = await gameSessionService.GetCfdDataForTeam(credentials.SessionId, credentials.TeamId);
		return View(cfd);
	}

	[HttpGet("7")]
	[HttpGet("7/0")]
	public async Task<IActionResult> Step7Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId);
		return View((credentials.SessionId, currentDay.Number));
	}

	[HttpGet("7/1")]
	public async Task<IActionResult> Step7Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, credentials.SessionId, credentials.TeamId);
		return View((credentials.SessionId, currentDay.Number));
	}

	[HttpPost("end-day")]
	public async Task EndDay(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		await teamService.PatchDayAsync(credentials, new EndDayCommand());
	}
}