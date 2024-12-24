using Core.Dtos;
using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.TeamMembers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.DayStepModels;

namespace WebApp.Controllers;

[Route("{gameSessionId:guid}/{teamId:guid}/api")]
public class ApiController : Controller
{
	private readonly IGameSessionService gameSessionService;
	private readonly ITeamService teamService;
	private readonly IDomainInteractionService domainInteractionService;

	public ApiController(
		IGameSessionService gameSessionService,
		ITeamService teamService,
		IDomainInteractionService domainInteractionService)
	{
		this.gameSessionService = gameSessionService;
		this.teamService = teamService;
		this.domainInteractionService = domainInteractionService;
	}

	[HttpPost("remove-user")]
	public async Task<bool> RemoveUser(Guid gameSessionId, Guid teamId, [FromBody] Guid userId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var removed = await gameSessionService.RemoveParticipantAsync(requestContext, gameSessionId, userId);
		return removed;
	}

	[HttpPost("update-team-name")]
	public async Task<bool> UpdateTeamName(
		Guid gameSessionId,
		Guid teamId,
		[FromBody] TeamNameUpdateDto teamNameUpdateDto)
	{
		var requestContext = RequestContextFactory.Build(Request);

		var updated = await gameSessionService.UpdateTeamName(
			gameSessionId,
			teamNameUpdateDto.TeamId,
			teamNameUpdateDto.TeamName);
		return updated;
	}

	[HttpPost("start-game")]
	public async Task StartGame(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		await gameSessionService.StartGameAsync(requestContext, gameSessionId);
	}

	[HttpPost("close-game")]
	public async Task CloseGame(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		await gameSessionService.CloseGameSession(gameSessionId);
	}

	[HttpPost("check-user-joined")]
	public async Task<UserDto?> CheckUserJoined(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		try
		{
			var currentUser = await gameSessionService.GetCurrentUser(requestContext);
			var team = await gameSessionService.GetCurrentTeam(requestContext, gameSessionId);
			return currentUser;
		}
		catch (InvalidOperationException e)
		{
			return null;
		}
	}

	[HttpPost("leave-game")]
	public async Task<Guid?> LeaveGame(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var isRemoved = await gameSessionService.RemoveParticipantAsync(requestContext, gameSessionId);
		return isRemoved ? requestContext.GetUserId() : null;
	}

	[HttpPost("another-team-roll")]
	public async Task<AnotherTeamDiceRollModel?> AnotherTeamRoll(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		if (currentDay.AwaitedCommands.All(x => x.CommandType != DayCommandType.WorkAnotherTeam))
		{
			return null;
		}
		
		await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new WorkAnotherTeamDayCommand());
		currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		return new AnotherTeamDiceRollModel
		{
			DiceNumber = currentDay.WorkAnotherTeamContainer!.DiceNumber,
			ScoresNumber = currentDay.WorkAnotherTeamContainer.ScoresNumber,
			TotalScores = currentDay.WorkAnotherTeamTotalScores
		};
	}

	[HttpPost("save-roles-transformation")]
	public async Task<DayDto?> SaveRolesTransformation(Guid gameSessionId, Guid teamId, [FromBody] RoleTransformationDto roleTransformationDto)
	{
		var requestContext = RequestContextFactory.Build(Request);
		
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		if (currentDay.AwaitedCommands.All(x => x.CommandType != DayCommandType.UpdateTeamRoles))
		{
			return null;
		}
		
		return await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new UpdateTeamRolesCommand
			{
				TeamMemberId = roleTransformationDto.TeamMemberId,
				To = Enum.Parse<TeamRole>(roleTransformationDto.RoleTo)
			});
	}

	[HttpGet("check-valid-cfd")]
	public async Task<bool?> CheckValidCfd(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		if (currentDay.AwaitedCommands.All(x => x.CommandType != DayCommandType.UpdateCfd))
		{
			return null;
		}
		
		var isValid = await gameSessionService.CheckValidCfd(requestContext, gameSessionId, teamId);
		return isValid;
	}

	[HttpGet("roll")]
	public async Task<DayDto?> RollDices(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		if (currentDay.AwaitedCommands.All(x => x.CommandType != DayCommandType.RollDice))
		{
			return null;
		}
		
		return await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new RollDiceCommand());
	}

	[HttpPost("update-release")]
	public async Task<DayDto?> UpdateRelease(Guid gameSessionId, Guid teamId, [FromBody] TicketModel ticketModel)
	{
		var requestContext = RequestContextFactory.Build(Request);
		
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		if (currentDay.AwaitedCommands.All(x => x.CommandType != DayCommandType.ReleaseTickets))
		{
			return null;
		}
		
		return await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			ReleaseTicketsCommand.Create(ticketModel.TicketId, ticketModel.Remove));
	}

	[HttpPost("update-sprint-backlog")]
	public async Task<bool?> UpdateSprintBacklog(Guid gameSessionId, Guid teamId, [FromBody] TicketModel ticketModel)
	{
		var requestContext = RequestContextFactory.Build(Request);

		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		if (currentDay.AwaitedCommands.All(x => x.CommandType != DayCommandType.UpdateSprintBacklog))
		{
			return null;
		}
		
		try
		{
			await teamService.PatchDayAsync(
				requestContext,
				gameSessionId,
				teamId,
				new UpdateSprintBacklogCommand
				{
					TicketIds = [ticketModel.TicketId], Remove = ticketModel.Remove
				});
			return true;
		}
		catch (Exception e)
		{
			return false;
		}
	}

	[HttpPost("update-cfd")]
	public async Task<DayDto?> UpdateCfd(Guid gameSessionId, Guid teamId, [FromBody] CfdEntryModel cfdEntryModel)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var patchType = Enum.Parse<UpdateCfdContainerPatchType>(cfdEntryModel.PatchType);
		
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		if (currentDay.AwaitedCommands.All(x => x.CommandType != DayCommandType.UpdateCfd))
		{
			return null;
		}
		
		return await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new UpdateCfdCommand { PatchType = patchType, Value = cfdEntryModel.Value });
	}

	[HttpPost("end-day")]
	public async Task<DayDto?> EndDay(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);

		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		if (currentDay.AwaitedCommands.All(x => x.CommandType != DayCommandType.EndDay))
		{
			return null;
		}
		
		return await teamService.PatchDayAsync(
			requestContext,
			gameSessionId,
			teamId,
			new EndDayCommand());
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
			"work-another-team" => currentDay.AwaitedCommands.Any(t => t.CommandType == DayCommandType.WorkAnotherTeam),
			"release-tickets" => currentDay.AwaitedCommands.Any(t => t.CommandType == DayCommandType.ReleaseTickets),
			"update-sprint-backlog" => currentDay.AwaitedCommands.Any(t => t.CommandType == DayCommandType.UpdateSprintBacklog),
			"update-cfd" => currentDay.AwaitedCommands.Any(t => t.CommandType == DayCommandType.UpdateCfd),
			"end-day" => currentDay.AwaitedCommands.Any(t => t.CommandType == DayCommandType.EndDay),
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
		
		if (awaitedCommands.Any(t => t.CommandType == DayCommandType.EndDay))
		{
			return "6/1";
		}

		throw new InvalidOperationException();
	}

	[HttpGet("should-lock-testers")]
	public async Task<bool> ShouldLockTesters(Guid gameSessionId, Guid teamId)
	{
		var requestContext = RequestContextFactory.Build(Request);
		var currentDay = await teamService.GetCurrentDayAsync(requestContext, gameSessionId, teamId);
		var shouldLockTesters = await gameSessionService.ShouldLockTestersForTeam(
			requestContext,
			gameSessionId,
			teamId,
			currentDay.Number);
		return shouldLockTesters;
	}

	[HttpGet("skip")]
	public async Task<IActionResult> Skip(
		Guid gameSessionId,
		Guid teamId,
		int dayTo,
		int step = 1)
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

			await UpdateCfd(gameSessionId, teamId, new CfdEntryModel { PatchType = "WithAnalysts", Value = i });
			await UpdateCfd(gameSessionId, teamId, new CfdEntryModel { PatchType = "WithProgrammers", Value = i + 1 });
			await UpdateCfd(gameSessionId, teamId, new CfdEntryModel { PatchType = "WithTesters", Value = i + 2 });
			await UpdateCfd(gameSessionId, teamId, new CfdEntryModel { PatchType = "ToDeploy", Value = i + 3 });
			await UpdateCfd(gameSessionId, teamId, new CfdEntryModel { PatchType = "Released", Value = i + 4 });

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