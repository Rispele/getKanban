﻿using Core.Dtos.Containers;
using Core.Dtos.Containers.RollDice;
using Core.Dtos.Containers.TeamMembers;
using Domain.Game.Days;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.RollDice;
using Domain.Game.Days.DayContainers.TeamMembers;

namespace Core.Dtos.Converters;

public class DayDtoConverter
{
	public DayDto Convert(Day day)
	{
		return new DayDto(
			day.AnalystsNumber,
			day.ProgrammersNumber,
			day.TestersNumber,
			Convert(day.WorkAnotherTeamContainer),
			Convert(day.TeamMembersContainer),
			Convert(day.DiceRollContainer, day.TeamMembersContainer),
			Convert(day.ReleaseTicketContainer),
			Convert(day.UpdateSprintBacklogContainer),
			Convert(day.UpdateCfdContainer));
	}

	private WorkAnotherTeamContainerDto? Convert(WorkAnotherTeamContainer? container)
	{
		if (container is null)
		{
			return null;
		}

		return new WorkAnotherTeamContainerDto
		{
			Version = container.Version,
			DiceNumber = container.DiceNumber,
			ScoresNumber = container.ScoresNumber
		};
	}

	private TeamMembersContainerDto Convert(TeamMembersContainer container)
	{
		return new TeamMembersContainerDto
		{
			Version = container.Version,
			TeamRoleUpdates = container.TeamMembers.Select(Convert).ToList()
		};
	}

	private TeamMemberDto Convert(TeamMember teamMember)
	{
		return new TeamMemberDto
		{
			Id = teamMember.Id,
			InitialRole = Convert(teamMember.InitialRole),
			CurrentRole = Convert(teamMember.CurrentRole)
		};
	}

	private TeamRoleDto Convert(TeamRole teamRole)
	{
		return teamRole switch
		{
			TeamRole.Analyst => TeamRoleDto.Analyst,
			TeamRole.Programmer => TeamRoleDto.Programmer,
			TeamRole.Tester => TeamRoleDto.Tester,
			_ => throw new ArgumentOutOfRangeException(nameof(teamRole), teamRole, null)
		};
	}

	private RollDiceContainerDto? Convert(RollDiceContainer? container, TeamMembersContainer teamMembersContainer)
	{
		if (container is null)
		{
			return null;
		}

		return new RollDiceContainerDto
		{
			Version = container.Version,
			DiceRollResults = container.DiceRollResults.Select(t => Convert(t, teamMembersContainer)).ToArray()
		};
	}

	private DiceRollResultDto Convert(DiceRollResult result, TeamMembersContainer teamMembersContainer)
	{
		var teamMember = teamMembersContainer.TeamMembers.Single(t => t.Id == result.TeamMemberId);
		return new DiceRollResultDto(
			result.TeamMemberId,
			Convert(teamMember.InitialRole),
			Convert(teamMember.CurrentRole),
			result.DiceNumber,
			result.Scores);
	}

	private ReleaseTicketContainerDto Convert(ReleaseTicketContainer container)
	{
		return new ReleaseTicketContainerDto
		{
			Version = container.Version,
			TicketIds = container.TicketIds
		};
	}

	private UpdateSprintBacklogContainerDto Convert(UpdateSprintBacklogContainer container)
	{
		return new UpdateSprintBacklogContainerDto
		{
			Version = container.Version,
			TicketIds = container.TicketIds
		};
	}

	private UpdateCfdContainerDto Convert(UpdateCfdContainer container)
	{
		return new UpdateCfdContainerDto
		{
			Version = container.Version,
			Released = container.Released,
			ToDeploy = container.ToDeploy,
			WithAnalysts = container.WithAnalysts,
			WithProgrammers = container.WithProgrammers,
			WithTesters = container.WithTesters
		};
	}
}