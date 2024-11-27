using Domain.DomainExceptions;
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

	[UsedImplicitly]
	private Day()
	{
	}

	internal Day(
		int number,
		Scenario scenario,
		List<DayCommandType> initiallyAwaitedEvents,
		int analystsNumber,
		int programmersNumber,
		int testersNumber)
	{
		Number = number;
		this.scenario = scenario;
		awaitedCommands = initiallyAwaitedEvents.Select(t => new AwaitedCommands(t)).ToList();

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

	private bool CanPostEvent(DayCommandType commandType)
	{
		return currentlyAwaitedEvents.Any(e => e.CommandType == commandType);
	}
}