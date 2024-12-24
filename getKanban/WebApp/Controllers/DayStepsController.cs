using Core.RequestContexts;
using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.DayStepModels;

namespace WebApp.Controllers;

[Route("{gameSessionId:guid}/{teamId:guid}/step")]
public class DayStepsController : Controller
{
	private readonly IGameSessionService gameSessionService;
	private readonly ITeamService teamService;
	private readonly IStatisticsService statisticsService;
	private readonly IDomainInteractionService domainInteractionService;

	public DayStepsController(
		IGameSessionService gameSessionService,
		ITeamService teamService,
		IStatisticsService statisticsService,
		IDomainInteractionService domainInteractionService)
	{
		this.gameSessionService = gameSessionService;
		this.teamService = teamService;
		this.statisticsService = statisticsService;
		this.domainInteractionService = domainInteractionService;
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

	[HttpGet("4")]
	[HttpGet("4/0")]
	public async Task<IActionResult> Step4Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

		var shouldShowReleaseTickets = currentDay.AwaitedCommands
			.Any(x => x is { CommandType: DayCommandType.ReleaseTickets });
		var shouldShowUpdateSprintBacklogTickets = currentDay.AwaitedCommands
			.Any(x => x is { CommandType: DayCommandType.UpdateSprintBacklog });

		return (shouldShowReleaseTickets, shouldShowUpdateSprintBacklogTickets) switch
		{
			(true, true) =>
				View(
					"Step4Stage0",
					await FillWithCredentialsAsync<StepModel>(requestContext, gameSessionId, teamId)),
			(true, false) =>
				View(
					"Step4Stage0",
					await FillWithCredentialsAsync<StepModel>(requestContext, gameSessionId, teamId)),
			(false, true) =>
				View(
					"Step5Stage0",
					await FillWithCredentialsAsync<StepModel>(
						requestContext,
						gameSessionId,
						teamId,
						currentDay.Number)),
			(false, false) =>
				Redirect($"/{gameSessionId}/{teamId}/step/6/0")
		};
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
					currentDay.Number));
		}

		var ticketIds = await domainInteractionService.GetTicketsToRelease(gameSessionId, teamId);
		var pageTypeNumber = (ticketIds.Any(x => x.id.Contains('S')) ? 1 : 0)
		                   + (ticketIds.Any(x => x.id.Contains('I')) ? 1 : 0)
		                   + (ticketIds.Any(x => x.id.Contains('E') || x.id.Contains('F')) ? 1 : 0);

		if (pageTypeNumber == 0)
		{
			return View(
				"Step5Stage0",
				await FillWithCredentialsAsync<StepModel>(
					requestContext,
					gameSessionId,
					teamId,
					currentDay.Number));
		}

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
			currentDay.Number);
		stepModel.TicketIds = ticketIds;
		stepModel.PageType = pageType;
		return View(stepModel);
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
			currentDay.Number);
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
			return Redirect($"/{gameSessionId}/{teamId}/step/6/0");
		}

		var ticketIds = await domainInteractionService.GetBacklogTickets(gameSessionId, teamId);
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
			currentDay.Number);
		stepModel.TicketIds = ticketIds;
		stepModel.PageType = pageType;
		return View(stepModel);
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
			currentDay.Number);
		ticketCheckStepModel.TicketIds = ticketIds;
		return View(ticketCheckStepModel);
	}

	[HttpGet("6")]
	[HttpGet("6/0")]
	public async Task<IActionResult> Step6Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);

		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		var teamStatistic = await statisticsService.CollectStatistic(gameSessionId, teamId);

		var stepModel = await FillWithCredentialsAsync<CfdTableStepModel>(
			requestContext,
			gameSessionId,
			teamId);
		var currentDayStatistic = teamStatistic.DayStatistics
			.SingleOrDefault(x => x.DayNumber == currentDay.Number);
		if (currentDayStatistic is not null)
		{
			stepModel.Released = currentDayStatistic.CfdStatistic.Released;
			stepModel.ToDeploy = currentDayStatistic.CfdStatistic.ToDeploy;
			stepModel.WithAnalysts = currentDayStatistic.CfdStatistic.WithAnalysts;
			stepModel.WithProgrammers = currentDayStatistic.CfdStatistic.WithProgrammers;
			stepModel.WithTesters = currentDayStatistic.CfdStatistic.WithTesters;
		}

		return View(stepModel);
	}

	[HttpGet("6/1")]
	public async Task<IActionResult> Step6Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);

		var teamStatistic = await statisticsService.CollectStatistic(gameSessionId, teamId);

		var stepModel = await FillWithCredentialsAsync<CfdGraphStepModel>(
			requestContext,
			gameSessionId,
			teamId);
		stepModel.TeamStatistic = teamStatistic;

		return View(stepModel);
	}

	[HttpGet("7/1")]
	public async Task<IActionResult> Step7Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var team = await gameSessionService.GetCurrentTeam(requestContext, gameSessionId);
		var day = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		var model = await FillWithCredentialsAsync<FinishGameStepModel>(requestContext, gameSessionId, teamId);

		model.ShouldFinishGame = team.IsTeamSessionEnded;
		model.IsLastDay = team.IsLastDay;
		model.EndDayEventMessage = day.EndDayEventMessage;
		return View(model);
	}

	[HttpGet("game-result")]
	public async Task<IActionResult> WinnersPage(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);

		var teamStatistic = await statisticsService.CollectStatistic(gameSessionId, teamId);

		var stepModel = await FillWithCredentialsAsync<CfdGraphStepModel>(
			requestContext,
			gameSessionId,
			teamId);
		stepModel.TeamStatistic = teamStatistic;

		return View(stepModel);
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