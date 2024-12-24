using Domain.DomainExceptions;
using Domain.Game.Days.Commands;
using Domain.Game.Days.Configurations;
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

	public IReadOnlyList<AwaitedCommands> CurrentlyAwaitedCommands => awaitedCommands
		.Where(@event => !@event.Removed)
		.ToList();

	public WorkAnotherTeamContainer? WorkAnotherTeamContainer { get; internal set; }
	public TeamMembersContainer TeamMembersContainer { get; } = null!;
	public RollDiceContainer? DiceRollContainer { get; internal set; }
	public ReleaseTicketContainer ReleaseTicketContainer { get; private set; } = null!;
	public UpdateSprintBacklogContainer UpdateSprintBacklogContainer { get; private set; } = null!;
	public UpdateCfdContainer UpdateCfdContainer { get; } = null!;

	// ReSharper disable once UnassignedGetOnlyAutoProperty
	public long Id { get; }

	public DaySettings DaySettings { get; } = null!;

	public int Number => DaySettings.Number;

	public long Timestamp { get; [UsedImplicitly] private set; }

	public DayStatus Status { get; private set; }

	[UsedImplicitly]
	private Day()
	{
	}

	internal Day(DaySettings settings, Scenario scenario)
	{
		DaySettings = settings;

		Status = DayStatus.InProcess;
		this.scenario = scenario;
		awaitedCommands = scenario.InitiallyAwaitedCommands.Select(t => new AwaitedCommands(t)).ToList();

		TeamMembersContainer = new TeamMembersContainer(
			settings.AnalystsCount,
			settings.ProgrammersCount,
			settings.TestersCount,
			settings.LockTesters);
		UpdateCfdContainer = new UpdateCfdContainer();
		ReleaseTicketContainer = new ReleaseTicketContainer(settings.CanReleaseNotImmediately);
		UpdateSprintBacklogContainer = new UpdateSprintBacklogContainer();
	}

	private bool CanPostEvent(DayCommandType commandType)
	{
		return CurrentlyAwaitedCommands.Any(e => e.CommandType == commandType);
	}

	internal void PostDayEvent(DayCommandType dayCommandType, object? parameters)
	{
		var (toAwait, toRemove) = scenario.GetNextAwaited(dayCommandType, parameters);
		CurrentlyAwaitedCommands.ForEach(
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

	internal void EndDay()
	{
		Status = DayStatus.Finished;
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
}