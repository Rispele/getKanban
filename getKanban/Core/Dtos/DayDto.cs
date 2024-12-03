
using Core.Dtos.Containers;
using Core.Dtos.Containers.RollDice;
using Core.Dtos.Containers.TeamMembers;
using Domain.Game.Days;

namespace Core.Dtos;

public class DayDto
{
	public int AnalystsNumber { get; }
	public int ProgrammersNumber { get; }
	public int TestersNumber { get; }
	public WorkAnotherTeamContainerDto? WorkAnotherTeamContainer { get; }
	public TeamMembersContainerDto TeamMembersContainer { get; }
	public RollDiceContainerDto? RollDiceContainer { get; }
	public ReleaseTicketContainerDto ReleaseTicketContainer { get; }
	public UpdateSprintBacklogContainerDto UpdateSprintBacklogContainer { get; }
	public UpdateCfdContainerDto UpdateCfdContainer { get; }

	public DayStatus Status { get; }

	public int Number { get; }
	
	public DayDto(
		int analystsNumber,
		int programmersNumber,
		int testersNumber,
		WorkAnotherTeamContainerDto? workAnotherTeamContainer,
		TeamMembersContainerDto teamMembersContainer,
		RollDiceContainerDto? rollDiceContainer,
		ReleaseTicketContainerDto releaseTicketContainer,
		UpdateSprintBacklogContainerDto updateSprintBacklogContainer,
		UpdateCfdContainerDto updateCfdContainer,
		DayStatus status,
		int number)
	{
		AnalystsNumber = analystsNumber;
		ProgrammersNumber = programmersNumber;
		TestersNumber = testersNumber;
		WorkAnotherTeamContainer = workAnotherTeamContainer;
		TeamMembersContainer = teamMembersContainer;
		RollDiceContainer = rollDiceContainer;
		ReleaseTicketContainer = releaseTicketContainer;
		UpdateSprintBacklogContainer = updateSprintBacklogContainer;
		UpdateCfdContainer = updateCfdContainer;
		Status = status;
		Number = number;
	}
}