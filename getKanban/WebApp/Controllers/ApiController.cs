using Core.Services.Contracts;
using Domain.Game.Days.Commands;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.TeamMembers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.DayStepModels;

namespace WebApp.Controllers;

[Route("{gameSessionId:guid}/{teamId:guid}/api")]
public class ApiController : Controller
{
	private readonly ITeamService teamService;
	private readonly IDomainInteractionService domainInteractionService;

	public ApiController(ITeamService teamService, IDomainInteractionService domainInteractionService)
	{
		this.teamService = teamService;
		this.domainInteractionService = domainInteractionService;
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

	public class RestartDayModel
	{
		public Guid SessionId { get; [UsedImplicitly] set; }
		public Guid TeamId { get; [UsedImplicitly] set; }
		public int DayNumber { get; [UsedImplicitly] set; }
	}
}