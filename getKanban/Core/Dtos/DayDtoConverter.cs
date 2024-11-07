﻿using Domain.Game.Days;
using Domain.Game.Days.DayContainers;

namespace Core.Dtos;

public class DayDtoConverter
{
	public DayDto Convert(Day day)
	{
		return new DayDto(
			day.AnalystsNumber,
			day.ProgrammersNumber,
			day.TestersNumber,
			Convert(day.WorkAnotherTeamContainer),
			Convert(day.UpdateTeamRolesContainer),
			Convert(day.RollDiceContainer),
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

	private UpdateTeamRolesContainerDto Convert(UpdateTeamRolesContainer container)
	{
		return new UpdateTeamRolesContainerDto
		{
			Version = container.Version,
			TeamRoleUpdates = container.TeamRoleUpdates.Select(t => (t.Id, t.From, t.To)).ToList()
		};
	}

	private RollDiceContainerDto? Convert(RollDiceContainer? container)
	{
		if (container is null)
		{
			return null;
		}
		
		return new RollDiceContainerDto
		{
			Version = container.Version,
			AnalystsDiceNumber = container.AnalystsDiceNumber,
			ProgrammersDiceNumber = container.ProgrammersDiceNumber,
			TestersDiceNumber = container.TestersDiceNumber,
			AnalystsScores = container.AnalystsScores,
			ProgrammersScores = container.ProgrammersScores,
			TestersScores = container.TestersScores,
		};
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