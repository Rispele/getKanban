using Core.Dtos.Containers;
using Core.Dtos.Containers.RollDice;
using Core.Dtos.Containers.TeamMembers;
using Domain.Game.Days;
using WebApp.Models;

namespace Core.Dtos;

public class DayDto
{
	public DayFullIdDto FullId { get; }

	public int AnalystsNumber { get; }
	public int ProgrammersNumber { get; }
	public int TestersNumber { get; }
	public int WorkAnotherTeamTotalScores { get; }
	public string EndDayEventMessage { get; }
	public WorkAnotherTeamContainerDto? WorkAnotherTeamContainer { get; }
	public TeamMembersContainerDto TeamMembersContainer { get; }
	public RollDiceContainerDto? RollDiceContainer { get; }
	public ReleaseTicketContainerDto ReleaseTicketContainer { get; }
	public UpdateSprintBacklogContainerDto UpdateSprintBacklogContainer { get; }
	public UpdateCfdContainerDto UpdateCfdContainer { get; }

	public IReadOnlyList<AwaitedEventsDto> AwaitedCommands { get; }

	public DayStatus Status { get; }

	public int Number { get; }

	public DayDto(
		DayFullIdDto fullId,
		int analystsNumber,
		int programmersNumber,
		int testersNumber,
		int workAnotherTeamTotalScores,
		WorkAnotherTeamContainerDto? workAnotherTeamContainer,
		TeamMembersContainerDto teamMembersContainer,
		RollDiceContainerDto? rollDiceContainer,
		ReleaseTicketContainerDto releaseTicketContainer,
		UpdateSprintBacklogContainerDto updateSprintBacklogContainer,
		UpdateCfdContainerDto updateCfdContainer,
		IReadOnlyList<AwaitedEventsDto> awaitedCommands,
		DayStatus status,
		int number,
		string endDayEventMessage)
	{
		FullId = fullId;
		AnalystsNumber = analystsNumber;
		ProgrammersNumber = programmersNumber;
		TestersNumber = testersNumber;
		WorkAnotherTeamTotalScores = workAnotherTeamTotalScores;
		WorkAnotherTeamContainer = workAnotherTeamContainer;
		TeamMembersContainer = teamMembersContainer;
		RollDiceContainer = rollDiceContainer;
		ReleaseTicketContainer = releaseTicketContainer;
		UpdateSprintBacklogContainer = updateSprintBacklogContainer;
		UpdateCfdContainer = updateCfdContainer;
		AwaitedCommands = awaitedCommands;
		Status = status;
		Number = number;
		EndDayEventMessage = endDayEventMessage;
	}
}