using Core.RequestContexts;
using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.TeamMembers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.DayStepModels;

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
	public async Task AnotherTeamRoll(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new WorkAnotherTeamDayCommand());
	}

	[HttpGet("{pageNumber:int}")]
	[HttpGet("{pageNumber:int}/{stageNumber:int}")]
	public async Task<IActionResult> InformationCard(
		Guid gameSessionId,
		Guid teamId,
		int pageNumber,
		int stageNumber = 0)
	{
		var requestContext = RequestContextFactory.Build(Request);
		return View(
			$"Step{pageNumber}Stage{stageNumber}",
			await FillWithCredentialsAsync<StepModel>(requestContext, gameSessionId, teamId));
	}

	[HttpGet("1")]
	[HttpGet("1/0")]
	public async Task<IActionResult> Step1Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

		var shouldRollForAnotherTeam = currentDay.AwaitedCommands
			.Any(x => x is { CommandType: DayCommandType.WorkAnotherTeam });

		var stepModel = await FillWithCredentialsAsync<StepModel>(requestContext, gameSessionId, teamId);
		return shouldRollForAnotherTeam
			? View("AnotherTeamRoll", stepModel)
			: View(stepModel);
	}

	[HttpGet("1/2")]
	public async Task<IActionResult> Step1Stage2(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

		var stepModel = await FillWithCredentialsAsync<RoleUpdateStepModel>(requestContext, gameSessionId, teamId);
		stepModel.TeamMemberDtos = currentDay.TeamMembersContainer.TeamRoleUpdates.ToList();

		return View(stepModel);
	}

	[HttpPost("save-roles-transformation")]
	public async Task SaveRolesTransformation(Guid gameSessionId, Guid teamId, [FromBody] string[] transformation)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var teamMemberId = long.Parse(transformation[0]);
		var roleTo = Enum.Parse<TeamRole>(transformation[1]);

		await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new UpdateTeamRolesCommand { TeamMemberId = teamMemberId, To = roleTo });
	}

	[HttpGet("2")]
	[HttpGet("2/0")]
	public async Task<IActionResult> Step2Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

		var stepModel = await FillWithCredentialsAsync<RollDicesStepModel>(requestContext, gameSessionId, teamId);
		stepModel.RollDiceContainerDto = currentDay.RollDiceContainer!;

		return View(stepModel);
	}

	[HttpGet("roll")]
	public async Task RollDices(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new RollDiceCommand());
	}

	[HttpGet("4/1")]
	public async Task<IActionResult> Step4Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

		var shouldShowTickets = currentDay.AwaitedCommands
			.Any(x => x is { CommandType: DayCommandType.ReleaseTickets });

		if (!shouldShowTickets)
		{
			return View(
				"Step5Stage0",
				await FillWithCredentialsAsync<StepModel>(
					requestContext,
					gameSessionId,
					teamId,
					dayNumber: currentDay.Number));
		}

		var ticketIds = await gameSessionService.GetTicketsToRelease(gameSessionId, teamId);
		var pageTypeNumber = (ticketIds.Any(x => x.id.Contains('S')) ? 1 : 0)
		                   + (ticketIds.Any(x => x.id.Contains('I')) ? 1 : 0)
		                   + (ticketIds.Any(x => x.id.Contains('E') || x.id.Contains('F')) ? 1 : 0);
		var pageType = pageTypeNumber switch
		{
			1 => "Single",
			2 => "Double",
			3 => "Triple",
			_ => throw new InvalidOperationException()
		};

		var stepModel = await FillWithCredentialsAsync<TicketChoiceStepModel>(
			requestContext,
			gameSessionId,
			teamId,
			dayNumber: currentDay.Number);
		stepModel.TicketIds = ticketIds;
		stepModel.PageType = pageType;
		return View(stepModel);
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
		await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			ReleaseTicketsCommand.Create(ticketModel.TicketId, ticketModel.Remove));
	}

	[HttpGet("4/2")]
	public async Task<IActionResult> Step4Stage2(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

		var ticketIds = currentDay.ReleaseTicketContainer.TicketIds.ToList();

		var ticketCheckStepModel = await FillWithCredentialsAsync<TicketCheckStepModel>(
			requestContext,
			gameSessionId,
			teamId,
			dayNumber: currentDay.Number);
		ticketCheckStepModel.TicketIds = ticketIds;
		return View(ticketCheckStepModel);
	}

	[HttpGet("5/1")]
	public async Task<IActionResult> Step5Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			gameSessionId,
			teamId);

		var shouldShowTickets = currentDay.AwaitedCommands
			.Any(x => x is { CommandType: DayCommandType.UpdateSprintBacklog });

		if (!shouldShowTickets)
		{
			return View(
				"Step6Stage0",
				await FillWithCredentialsAsync<StepModel>(
					requestContext,
					gameSessionId,
					teamId,
					dayNumber: currentDay.Number));
		}

		var ticketIds = await gameSessionService.GetBacklogTickets(gameSessionId, teamId);
		var pageTypeNumber = (ticketIds.Any(x => x.id.Contains('S')) ? 1 : 0) +
		                     (ticketIds.Any(x => x.id.Contains('I')) ? 1 : 0) +
		                     (ticketIds.Any(x => x.id.Contains('E') || x.id.Contains('F')) ? 1 : 0);
		var pageType = pageTypeNumber switch
		{
			1 => "Single",
			2 => "Double",
			3 => "Triple",
			_ => throw new InvalidOperationException()
		};

		var stepModel = await FillWithCredentialsAsync<TicketChoiceStepModel>(
			requestContext,
			gameSessionId,
			teamId,
			dayNumber: currentDay.Number);
		stepModel.TicketIds = ticketIds;
		stepModel.PageType = pageType;
		return View(stepModel);
	}

	[HttpPost("update-sprint-backlog")]
	public async Task UpdateSprintBacklog([FromBody] TicketModel ticketModel, Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);

		await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new UpdateSprintBacklogCommand
			{
				TicketIds = [ticketModel.TicketId], Remove = ticketModel.Remove
			});
	}

	[HttpGet("5/2")]
	public async Task<IActionResult> Step5Stage2(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

		var ticketIds = currentDay.UpdateSprintBacklogContainer.TicketIds.ToList();
		var ticketCheckStepModel = await FillWithCredentialsAsync<TicketCheckStepModel>(
			requestContext,
			gameSessionId,
			teamId,
			dayNumber: currentDay.Number);
		ticketCheckStepModel.TicketIds = ticketIds;
		return View(ticketCheckStepModel);
	}

	[HttpPost("update-cfd")]
	public async Task UpdateCfd([FromBody] CfdDayDataModel cfdDayDataModel, Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);

		await PatchDayAsync(UpdateCfdContainerPatchType.Released, cfdDayDataModel.Released);
		await PatchDayAsync(UpdateCfdContainerPatchType.ToDeploy, cfdDayDataModel.ToDeploy);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithTesters, cfdDayDataModel.WithTesters);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithProgrammers, cfdDayDataModel.WithProgrammers);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithAnalysts, cfdDayDataModel.WithAnalysts);
		return;

		async Task PatchDayAsync(UpdateCfdContainerPatchType patchType, int value)
		{
			await teamService.PatchDayAsync(
				requestContext,
				gameSessionId,
				teamId,
				new UpdateCfdCommand
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
		
		var cfdGraphDto = await gameSessionService.GetCfdDataForTeam(gameSessionId, teamId);
		
		var cfdGraphStepModel = await FillWithCredentialsAsync<CfdGraphStepModel>(
			requestContext,
			gameSessionId,
			teamId);
		cfdGraphStepModel.CfdGraphDto = cfdGraphDto;
		
		return View(cfdGraphStepModel);
	}

	[HttpPost("end-day")]
	public async Task EndDay(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		
		await teamService.PatchDayAsync(requestContext, gameSessionId, teamId, new EndDayCommand());
	}

	private async Task<T> FillWithCredentialsAsync<T>(
		RequestContext requestContext,
		Guid gameSessionId,
		Guid teamId,
		int? dayNumber = null,
		string? teamName = null)
		where T : StepModel, new()
	{
		teamName ??= await gameSessionService.GetTeamName(gameSessionId, teamId);
		if (dayNumber is null)
		{
			var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
			dayNumber = currentDay.Number;
		}

		return new T
		{
			GameSessionId = gameSessionId,
			TeamId = teamId,
			TeamName = teamName,
			DayNumber = dayNumber.Value
		};
	}
}