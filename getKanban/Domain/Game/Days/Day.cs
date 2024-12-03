﻿using Domain.DomainExceptions;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.RollDice;
using Domain.Game.Days.DayContainers.TeamMembers;
using Domain.Game.Days.Scenarios;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days;

[EntityTypeConfiguration(typeof(DayEntityTypeConfiguration))]
public class Day
{
	private readonly List<AwaitedCommands> awaitedCommands = null!;
	private readonly Scenario scenario = null!;

	public int AnalystsNumber { get; }
	public int ProgrammersNumber { get; }
	public int TestersNumber { get; }

	private IEnumerable<AwaitedCommands> currentlyAwaitedEvents => awaitedCommands
		.Where(@event => !@event.Removed);

	public WorkAnotherTeamContainer? WorkAnotherTeamContainer { get; internal set; }
	public TeamMembersContainer TeamMembersContainer { get; } = null!;
	public RollDiceContainer? DiceRollContainer { get; internal set; }
	public ReleaseTicketContainer ReleaseTicketContainer { get; private set; } = null!;
	public UpdateSprintBacklogContainer UpdateSprintBacklogContainer { get; private set; } = null!;
	public UpdateCfdContainer UpdateCfdContainer { get; } = null!;

	public long Id { get; }

	public int Number { get; }

	public long Timestamp { get; [UsedImplicitly] private set; }

	public DayStatus Status { get; set; }

	[UsedImplicitly]
	private Day()
	{
	}

	internal Day(
		int number,
		Scenario scenario,
		int analystsNumber,
		int programmersNumber,
		int testersNumber)
	{
		Number = number;
		Status = DayStatus.InProcess;
		this.scenario = scenario;
		awaitedCommands = scenario.InitiallyAwaitedCommands.Select(t => new AwaitedCommands(t)).ToList();

		AnalystsNumber = analystsNumber;
		ProgrammersNumber = programmersNumber;
		TestersNumber = testersNumber;

		TeamMembersContainer = new TeamMembersContainer(AnalystsNumber, ProgrammersNumber, TestersNumber);
		UpdateCfdContainer = new UpdateCfdContainer();
		ReleaseTicketContainer = new ReleaseTicketContainer();
		UpdateSprintBacklogContainer = new UpdateSprintBacklogContainer();
	}

	internal void PostDayEvent(DayCommandType dayCommandType, object? parameters)
	{
		var (toAwait, toRemove) = scenario.GetNextAwaited(dayCommandType, parameters);
		currentlyAwaitedEvents.ForEach(
			e =>
			{
				if (toRemove.Contains(e.CommandType))
				{
					e.MarkRemoved();
				}
			});

		awaitedCommands.AddRange(toAwait.Select(e => new AwaitedCommands(e)));
	}

	internal void EnsureCanPostEvent(DayCommandType commandType)
	{
		if (CanPostEvent(commandType))
		{
			return;
		}

		throw new DayEventNotAwaitedException($"{commandType} is not awaited");
	}

	internal bool IsCfdValid(UpdateCfdContainer previousDayCfd)
	{
		if (UpdateCfdContainer
		    is { Released: null }
		    or { ToDeploy: null }
		    or { WithTesters: null }
		    or { WithProgrammers: null }
		    or { WithAnalysts: null })
		{
			return false;
		}

		var currentSumToValidate = UpdateCfdContainer.Released! + UpdateCfdContainer.ToDeploy!;
		var previousSumToValidate = previousDayCfd.Released! + previousDayCfd.ToDeploy!;
		if (!ValidateArgumentsSum())
		{
			return false;
		}

		currentSumToValidate += UpdateCfdContainer.WithTesters;
		previousSumToValidate += previousDayCfd.WithTesters!;
		if (!ValidateArgumentsSum())
		{
			return false;
		}

		currentSumToValidate += UpdateCfdContainer.WithProgrammers;
		previousSumToValidate += previousDayCfd.WithProgrammers!;
		if (!ValidateArgumentsSum())
		{
			return false;
		}

		currentSumToValidate += UpdateCfdContainer.WithAnalysts;
		previousSumToValidate += previousDayCfd.WithAnalysts!;
		if (!ValidateArgumentsSum())
		{
			return false;
		}

		return true;

		bool ValidateArgumentsSum()
		{
			return !(currentSumToValidate < previousSumToValidate);
		}
	}

	private bool CanPostEvent(DayCommandType commandType)
	{
		return currentlyAwaitedEvents.Any(e => e.CommandType == commandType);
	}
}