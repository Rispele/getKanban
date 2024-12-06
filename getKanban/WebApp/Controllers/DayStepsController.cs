using Core.Dtos;
using Core.Dtos.Containers;
using Core.Dtos.Containers.RollDice;
using Core.Services.Contracts;
using Domain.Game;
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
	public async Task AnotherTeamRoll()
	{
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		await teamService.PatchDayAsync(credentials, new WorkAnotherTeamDayCommand());
	}

	[HttpGet("{pageNumber:int}")]
	[HttpGet("{pageNumber:int}/{stageNumber:int}")]
	public async Task<IActionResult> InformationCard(int pageNumber, int stageNumber = 0) =>
		View($"Step{pageNumber}Stage{stageNumber}", await FillWithCredentials(Request, new StepModel()));

	[HttpGet("1")]
	[HttpGet("1/0")]
	public async Task<IActionResult> Step1Stage0(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			credentials.SessionId,
			credentials.TeamId);

		var shouldRollForAnotherTeam = currentDay
			.AwaitedCommands.Any(x => x is { CommandType: DayCommandType.WorkAnotherTeam });

		var stepModel = await FillWithCredentials(Request, new StepModel(), credentials);
		return shouldRollForAnotherTeam
			? View("AnotherTeamRoll", stepModel)
			: View(stepModel);
	}

	[HttpGet("1/2")]
	public async Task<IActionResult> Step1Stage2(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			credentials.SessionId,
			credentials.TeamId);

		var roleUpdateStepModel = await FillWithCredentials(Request, new RoleUpdateStepModel(), credentials);
		var teamMembers = currentDay.TeamMembersContainer.TeamRoleUpdates.ToList();
		roleUpdateStepModel.TeamMemberDtos = teamMembers;
		return View(roleUpdateStepModel);
	}

	[HttpPost("save-roles-transformation")]
	public async Task SaveRolesTransformation([FromBody] string[] transformation)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(requestContext);

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
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			credentials.SessionId,
			credentials.TeamId);

		var rollDicesStepModel = await FillWithCredentials(Request, new RollDicesStepModel(), credentials);
		rollDicesStepModel.RollDiceContainerDto = currentDay.RollDiceContainer!;
		return View(rollDicesStepModel);
	}

	[HttpGet("roll")]
	public async Task RollDices(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		await teamService.PatchDayAsync(credentials, new RollDiceCommand());
	}

	[HttpGet("4/1")]
	public async Task<IActionResult> Step4Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		var team = await gameSessionService.GetCurrentTeam(requestContext, credentials.SessionId);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			credentials.SessionId,
			credentials.TeamId);

		var shouldShowTickets = currentDay
			.AwaitedCommands.Any(x => x is { CommandType: DayCommandType.ReleaseTickets });

		if (!shouldShowTickets)
		{
			var stepModel = new StepModel()
			{
				TeamId = credentials.SessionId,
				TeamName = team.Name,
				DayNumber = currentDay.Number
			};
			return View("Step5Stage0", stepModel);
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
		};

		var ticketChoiceStepModel = new TicketChoiceStepModel()
		{
			TeamId = credentials.SessionId,
			TeamName = team.Name,
			DayNumber = currentDay.Number,
			PageType = pageType!,
			TicketIds = ticketIds
		};
		return View(ticketChoiceStepModel);
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
		var credentials = await gameSessionService.GetUserCredentials(requestContext);

		await teamService.PatchDayAsync(
			credentials,
			ReleaseTicketsCommand.Create(ticketModel.TicketId, ticketModel.Remove));
	}

	[HttpGet("4/2")]
	public async Task<IActionResult> Step4Stage2(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			credentials.SessionId,
			credentials.TeamId);

		var ticketIds = currentDay.ReleaseTicketContainer.TicketIds.ToList();
		var ticketCheckStepModel = await FillWithCredentials(Request, new TicketCheckStepModel(), credentials);
		ticketCheckStepModel.TicketIds = ticketIds;
		return View(ticketCheckStepModel);
	}

	[HttpGet("5/1")]
	public async Task<IActionResult> Step5Stage1(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		var team = await gameSessionService.GetCurrentTeam(requestContext, credentials.SessionId);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			credentials.SessionId,
			credentials.TeamId);

		var shouldShowTickets = (await teamService.GetCurrentDayAsync(
				requestContext,
				credentials.SessionId,
				credentials.TeamId))
			.AwaitedCommands.Any(x => x is { CommandType: DayCommandType.UpdateSprintBacklog });

		if (!shouldShowTickets)
		{
			var stepModel = new StepModel()
			{
				TeamId = credentials.SessionId,
				TeamName = team.Name,
				DayNumber = currentDay.Number
			};
			return View("Step6Stage0", stepModel);
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

		var ticketChoiceStepModel = new TicketChoiceStepModel()
		{
			TeamId = credentials.SessionId,
			TeamName = team.Name,
			DayNumber = currentDay.Number,
			PageType = pageType!,
			TicketIds = ticketIds
		};
		return View(ticketChoiceStepModel);
	}

	[HttpPost("update-sprint-backlog")]
	public async Task UpdateSprintBacklog([FromBody] TicketModel ticketModel, Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var credentials = await gameSessionService.GetUserCredentials(requestContext);

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
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			credentials.SessionId,
			credentials.TeamId);

		var ticketIds = currentDay.UpdateSprintBacklogContainer.TicketIds.ToList();
		var ticketCheckStepModel = await FillWithCredentials(Request, new TicketCheckStepModel());
		ticketCheckStepModel.TicketIds = ticketIds;
		return View(ticketCheckStepModel);
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
		var credentials = await gameSessionService.GetUserCredentials(requestContext);
		var cfdGraphDto = await gameSessionService.GetCfdDataForTeam(credentials.SessionId, credentials.TeamId);
		var cfdGraphStepModel = await FillWithCredentials(Request, new CfdGraphStepModel(), credentials);
		cfdGraphStepModel.CfdGraphDto = cfdGraphDto;
		return View(cfdGraphStepModel);
	}

	[HttpPost("end-day")]
	public async Task EndDay(Guid gameSessionId, Guid teamId)
	{
		var credentials = await gameSessionService.GetUserCredentials(RequestContextFactory.Build(Request));
		await teamService.PatchDayAsync(credentials, new EndDayCommand());
	}

	private async Task<T> FillWithCredentials<T>(
		HttpRequest request,
		T stepModel,
		UserCredentialsDto? userCredentialsDto = null)
		where T : StepModel
	{
		var requestContext = RequestContextFactory.Build(request);
		var credentials = userCredentialsDto
		               ?? await gameSessionService.GetUserCredentials(requestContext);
		var team = await gameSessionService.GetCurrentTeam(requestContext, credentials.SessionId);
		var currentDay = await teamService.GetCurrentDayAsync(
			requestContext,
			credentials.SessionId,
			credentials.TeamId);
		stepModel.GameSessionId = credentials.SessionId;
		stepModel.TeamId = credentials.TeamId;
		stepModel.TeamName = team.Name;
		stepModel.DayNumber = currentDay.Number;
		return stepModel;
	}
}