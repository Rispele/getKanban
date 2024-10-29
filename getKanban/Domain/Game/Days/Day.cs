using Domain.DomainExceptions;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.EndDayDayEvent;
using Domain.Game.Days.DayEvents.ReleaseTicketDayEvent;
using Domain.Game.Days.DayEvents.RollDiceDayEvent;
using Domain.Game.Days.DayEvents.UpdateCfdDayEvent;
using Domain.Game.Days.DayEvents.UpdateSprintBacklogDayEvent;
using Domain.Game.Days.DayEvents.UpdateTeamRolesDayEvent;
using Domain.Game.Days.DayEvents.WorkAnotherTeamDayEvent;

namespace Domain.Game.Days;

public class Day
{
	private readonly int analystsCount;
	private readonly List<AwaitedEvent> awaitedEvents;
	private readonly List<DayEvent> events;
	private readonly int programmersCount;
	private readonly bool shouldRelease;
	private readonly bool shouldUpdateSprintBacklog;
	private readonly int testersCount;

	public long Number { get; }

	private int LastEventId => events.Max(t => t.Id);

	private IEnumerable<DayEvent> ExistingEvents => events.Where(@event => !@event.IsRemoved);

	public Day(
		int number,
		int analystsCount,
		int programmersCount,
		int testersCount,
		bool anotherTeamAppeared,
		bool shouldRelease,
		bool shouldUpdateSprintBacklog)
	{
		this.analystsCount = analystsCount;
		this.programmersCount = programmersCount;
		this.testersCount = testersCount;
		this.shouldRelease = shouldRelease;
		this.shouldUpdateSprintBacklog = shouldUpdateSprintBacklog;
		Number = number;

		events = new List<DayEvent>();
		awaitedEvents = new List<AwaitedEvent>();

		if (anotherTeamAppeared)
		{
			awaitedEvents.Add(new AwaitedEvent(DayEventType.WorkAnotherTeam));
		}
		else
		{
			AwaitFirstStepEvents();
		}
	}

	public int RollDiceForAnotherTeam()
	{
		const string notExpectedMessage = "Roll dice for Petya is not awaited this day or state";
		var workAnotherTeamEvent = GetAwaitedEventOrThrow(DayEventType.WorkAnotherTeam, notExpectedMessage);

		var diceRoller = new DiceRoller(new Random());
		var diceNumber = diceRoller.RollDice();
		var diceScores = MapAnotherTeam(diceNumber);
		events.Add(new WorkAnotherTeamDayEvent(diceNumber, diceScores, LastEventId + 1));

		workAnotherTeamEvent.MarkRemoved();
		AwaitFirstStepEvents();

		return diceScores;
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		const string notExpectedMessage = "Update team roles is not awaited this state";
		GetAwaitedEventOrThrow(DayEventType.UpdateTeamRoles, notExpectedMessage);

		events.Add(new UpdateTeamRolesDayEvent(from, to, LastEventId + 1));
	}

	public void RollDices()
	{
		const string notExpectedMessage = "Dice rolling is not awaited";
		var updateTeamRolesEvent = GetAwaitedEventOrThrow(DayEventType.UpdateTeamRoles, notExpectedMessage);
		var rollDiceEvent = GetAwaitedEventOrThrow(DayEventType.RollDice, notExpectedMessage);

		var swapRoleEvents = events
			.Where(@event => @event.Type == DayEventType.UpdateTeamRoles)
			.Where(@event => !@event.IsRemoved)
			.Cast<UpdateTeamRolesDayEvent>()
			.GroupBy(@event => @event.From)
			.ToDictionary(grouping => grouping.Key, grouping => grouping.Select(@event => @event.To).ToArray());

		var diceRoller = new DiceRoller(new Random());
		var (analystsDiceNumber, analystsScores) = RollDiceForRole(analystsCount, TeamRole.Analyst);
		var (programmersDiceNumber, programmersScores) = RollDiceForRole(programmersCount, TeamRole.Programmer);
		var (testersDiceNumber, testersScores) = RollDiceForRole(testersCount, TeamRole.Tester);

		var @event = new RollDiceDayEvent(
			analystsDiceNumber,
			programmersDiceNumber,
			testersDiceNumber,
			analystsScores,
			programmersScores,
			testersScores,
			LastEventId + 1);
		events.Add(@event);

		updateTeamRolesEvent.MarkRemoved();
		rollDiceEvent.MarkRemoved();

		AwaitEvents(
			shouldRelease
				? DayEventType.ReleaseTickets
				: shouldUpdateSprintBacklog
					? DayEventType.UpdateSprintBacklog
					: DayEventType.UpdateCfd);

		(int[] diceNumber, int[] diceScores) RollDiceForRole(int roleSize, TeamRole role)
		{
			var swaps = swapRoleEvents.GetValueOrDefault(role, Array.Empty<TeamRole>());
			var diceNumber = new int[roleSize];
			var diceScores = new int[roleSize];
			for (var i = 0; i < roleSize; i++)
			{
				var asRole = i < swaps.Length ? swaps[i] : role;
				diceNumber[i] = diceRoller.RollDice();
				diceScores[i] = MapAnalyst(diceNumber[i], asRole);
			}

			return (diceNumber, diceScores);
		}
	}

	public void ReleaseTickets(string[] ticketIds)
	{
		const string notExpectedMessage = "Ticket release is not awaited";
		var releaseTicketEvent = GetAwaitedEventOrThrow(DayEventType.ReleaseTickets, notExpectedMessage);

		events.Add(new ReleaseTicketDayEvent(ticketIds, LastEventId + 1));

		releaseTicketEvent.MarkRemoved();

		AwaitEvents(shouldUpdateSprintBacklog ? DayEventType.UpdateSprintBacklog : DayEventType.UpdateCfd);
	}

	public void UpdateSprintBacklog(string[] ticketIds)
	{
		const string notExpectedMessage = "Sprint Backlog update is not awaited";
		var updateSprintBacklogEvent = GetAwaitedEventOrThrow(DayEventType.UpdateSprintBacklog, notExpectedMessage);

		events.Add(new UpdateSprintBacklogDayEvent(ticketIds, LastEventId + 1));

		updateSprintBacklogEvent.MarkRemoved();
		AwaitEvents(DayEventType.UpdateCfd);
	}

	public void UpdateCfd(int released, int readyToDeploy, int withTesters, int withProgrammers, int withAnalysts)
	{
		const string notExpectedMessage = "CFD update is not awaited";
		var updateCfdEvent = GetAwaitedEventOrThrow(DayEventType.UpdateCfd, notExpectedMessage);

		var releasedThisDay = (ReleaseTicketDayEvent?)ExistingEvents
			.SingleOrDefault(@event => @event.Type == DayEventType.ReleaseTickets);
		var totalReleased = released + releasedThisDay?.TicketIds.Count ?? 0;

		events.Add(
			new UpdateCfdDayEvent(
				totalReleased,
				readyToDeploy,
				withTesters,
				withProgrammers,
				withAnalysts,
				LastEventId + 1));
		updateCfdEvent.MarkRemoved();
		AwaitEvents(DayEventType.EndDay);
	}

	public void EndDay()
	{
		const string notExpectedMessage = "End day is not awaited";
		var endDayEvent = GetAwaitedEventOrThrow(DayEventType.EndDay, notExpectedMessage);

		events.Add(new EndDayDayEvent(LastEventId + 1));

		endDayEvent.MarkRemoved();
	}

	private void AwaitFirstStepEvents()
	{
		AwaitEvents(DayEventType.UpdateTeamRoles, DayEventType.RollDice);
	}


	private void AwaitEvents(params DayEventType[] eventTypes)
	{
		foreach (var eventType in eventTypes)
		{
			awaitedEvents.Add(new AwaitedEvent(eventType));
		}
	}

	private AwaitedEvent GetAwaitedEventOrThrow(DayEventType type, string errorMessage)
	{
		var expectedEvent = awaitedEvents
			.Where(@event => !@event.Removed)
			.SingleOrDefault(@event => @event.EventType == type);

		return expectedEvent ?? throw new DayEventNotAwaitedException(errorMessage);
	}

	#region Mappings

#pragma warning disable CS8509
	private static int MapAnalyst(int diceNumber, TeamRole asRole)
	{
		ValidateDiceNumber(diceNumber);

		switch (asRole)
		{
			case TeamRole.Analyst:
				return diceNumber switch
				{
					1 => 3,
					2 => 4,
					3 => 4,
					4 => 5,
					5 => 5,
					6 => 6
				};
			case TeamRole.Programmer:
				return diceNumber switch
				{
					1 => 1,
					2 => 2,
					3 => 0,
					4 => 2,
					5 => 1,
					6 => 3
				};
			case TeamRole.Tester:
				return diceNumber switch
				{
					1 => 2,
					2 => 1,
					3 => 2,
					4 => 3,
					5 => 3,
					6 => 1
				};
			default:
				throw new ArgumentOutOfRangeException(nameof(asRole), asRole, "Unknown role");
		}
	}

	private static int MapProgrammer(int diceNumber, TeamRole asRole)
	{
		ValidateDiceNumber(diceNumber);

		switch (asRole)
		{
			case TeamRole.Analyst:
				return diceNumber switch
				{
					1 => 2,
					2 => 2,
					3 => 1,
					4 => 1,
					5 => 3,
					6 => 1
				};
			case TeamRole.Programmer:
				return diceNumber switch
				{
					1 => 3,
					2 => 3,
					3 => 4,
					4 => 4,
					5 => 5,
					6 => 6
				};
			case TeamRole.Tester:
				return diceNumber switch
				{
					1 => 1,
					2 => 1,
					3 => 2,
					4 => 0,
					5 => 2,
					6 => 3
				};
			default:
				throw new ArgumentOutOfRangeException(nameof(asRole), asRole, "Unknown role");
		}
	}

	private static int MapTester(int diceNumber, TeamRole asRole)
	{
		ValidateDiceNumber(diceNumber);

		switch (asRole)
		{
			case TeamRole.Analyst:
				return diceNumber switch
				{
					1 => 1,
					2 => 0,
					3 => 2,
					4 => 1,
					5 => 2,
					6 => 3
				};
			case TeamRole.Programmer:
				return diceNumber switch
				{
					1 => 2,
					2 => 1,
					3 => 1,
					4 => 2,
					5 => 3,
					6 => 1
				};
			case TeamRole.Tester:
				return diceNumber switch
				{
					1 => 3,
					2 => 4,
					3 => 4,
					4 => 5,
					5 => 5,
					6 => 6
				};
			default:
				throw new ArgumentOutOfRangeException(nameof(asRole), asRole, "Unknown role");
		}
	}

	private static int MapAnotherTeam(int diceNumber)
	{
		ValidateDiceNumber(diceNumber);

		return diceNumber switch
		{
			1 => 0,
			2 => 2,
			3 => 3,
			4 => 4,
			5 => 5,
			6 => 15
		};
	}

	private static void ValidateDiceNumber(int diceNumber)
	{
		if (diceNumber is < 1 or > 6)
		{
			throw new InvalidOperationException(
				$"Invalid {nameof(diceNumber)}:{diceNumber}. Should be between 1 and 6");
		}
	}
#pragma warning restore CS8509

	#endregion
}