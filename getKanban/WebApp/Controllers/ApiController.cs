using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.TeamMembers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;
using WebApp.Models;
using WebApp.Models.DayStepModels;

namespace WebApp.Controllers;

[Route("{gameSessionId:guid}/{teamId:guid}/api")]
public class ApiController : Controller
{
	private readonly ITeamService teamService;
	private readonly IDomainInteractionService domainInteractionService;
	private readonly IHubContext<TeamSessionHub> teamSessionHub;

	public ApiController(ITeamService teamService, IDomainInteractionService domainInteractionService)
	{
		this.teamService = teamService;
		this.domainInteractionService = domainInteractionService;
	}
	
	[HttpPost("another-team-roll")]
	public async Task<AnotherTeamDiceRollModel> AnotherTeamRoll(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new WorkAnotherTeamDayCommand());
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		return new AnotherTeamDiceRollModel()
		{
			DiceNumber = currentDay.WorkAnotherTeamContainer!.DiceNumber,
			ScoresNumber = currentDay.WorkAnotherTeamContainer.ScoresNumber
		};
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
	
	[HttpPost("update-cfd")]
	public async Task UpdateCfd([FromBody] CfdDayDataModel cfdDayDataModel, Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);

		await PatchDayAsync(UpdateCfdContainerPatchType.Released, cfdDayDataModel.Released);
		await PatchDayAsync(UpdateCfdContainerPatchType.ToDeploy, cfdDayDataModel.ToDeploy);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithTesters, cfdDayDataModel.WithTesters);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithProgrammers, cfdDayDataModel.WithProgrammers);
		await PatchDayAsync(UpdateCfdContainerPatchType.WithAnalysts, cfdDayDataModel.WithAnalysts);
		
		// await ... getCurrentDay -> awaitedCommands
		
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
	
	[HttpPost("end-day")]
	public async Task EndDay(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		
		await teamService.PatchDayAsync(requestContext, gameSessionId, teamId, new EndDayCommand());
	}

	[HttpPost("restart-day")]
	public async Task RestartDay([FromBody] RestartDayModel restartDayModel)
	{
		await domainInteractionService.RestartDay(
			restartDayModel.SessionId,
			restartDayModel.TeamId,
			restartDayModel.DayNumber);
	}

	[HttpGet("check-available")]
	public async Task<bool> CheckPageAvailable(Guid gameSessionId, Guid teamId, string @event)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
			
		return @event switch
		{
			"roll" => currentDay.AwaitedCommands.Any(t => t.CommandType == DayCommandType.RollDice),
			"update-role" => currentDay.AwaitedCommands.Any(t => t.CommandType == DayCommandType.UpdateTeamRoles),
			"another-team-roll" => currentDay.AwaitedCommands.Any(t => t.CommandType == DayCommandType.WorkAnotherTeam),
			_ => false
		};
	}

	[HttpGet("get-current-step")]
	public async Task<string> GetCurrentStep(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		var awaitedCommands = currentDay.AwaitedCommands;
		if (awaitedCommands.Any(t => t.CommandType == DayCommandType.WorkAnotherTeam))
		{
			return "1/0";
		}
		if (awaitedCommands.Any(t => t.CommandType == DayCommandType.UpdateTeamRoles))
		{
			return "1/1";
		}
		if (awaitedCommands.Any(t => t.CommandType == DayCommandType.ReleaseTickets))
		{
			return "4/0";
		}
		if (awaitedCommands.Any(t => t.CommandType == DayCommandType.UpdateSprintBacklog))
		{
			return "5/0";
		}
		if (awaitedCommands.Any(t => t.CommandType == DayCommandType.UpdateCfd))
		{
			return "6/0";
		}

		throw new InvalidOperationException();
	}

	[HttpGet("skip")]
	public async Task<IActionResult> Skip(Guid gameSessionId, Guid teamId, int dayTo, int step = 1)
	{
		for (var i = 9; i < dayTo + 1; i++)
		{
			if (i == dayTo && step == 1)
			{
				return Redirect($"/{gameSessionId}/{teamId}/step/{step}");
			}
			
			var requestContext = RequestContextFactory.Build(Request);
			var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);

			var shouldRollForAnotherTeam = currentDay.AwaitedCommands
				.Any(x => x is { CommandType: DayCommandType.WorkAnotherTeam });
			if (shouldRollForAnotherTeam)
			{
				await AnotherTeamRoll(gameSessionId, teamId);
			}

			await RollDices(gameSessionId, teamId);
			
			if (i == dayTo && step == 2)
			{
				return Redirect($"/{gameSessionId}/{teamId}/step/{step}");
			}
			
			// Выбор тикетов скипаем в любом случае

			if (i == dayTo && step == 5)
			{
				return Redirect($"/{gameSessionId}/{teamId}/step/{step}");
			}
			
			await UpdateCfd(new CfdDayDataModel()
			{
				WithAnalysts = i,
				WithProgrammers = i + 1,
				WithTesters = i + 2,
				ToDeploy = i + 3,
				Released = i + 4
			}, gameSessionId, teamId);

			if (i == dayTo && step == 7)
			{
				return Redirect($"/{gameSessionId}/{teamId}/step/{step}");
			}

			await EndDay(gameSessionId, teamId);
		}

		return null;
	}

	[HttpGet("error")]
	public async Task<IActionResult> Error(string message)
	{
		return View("Error", message);
	}

	public class RestartDayModel
	{
		public Guid SessionId { get; [UsedImplicitly] set; }
		public Guid TeamId { get; [UsedImplicitly] set; }
		public int DayNumber { get; [UsedImplicitly] set; }
	}
}