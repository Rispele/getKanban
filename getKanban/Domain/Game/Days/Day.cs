using Domain.DomainExceptions;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.RollDiceDayEvent;
using Domain.Game.Days.DayEvents.UpdateTeamRolesDayEvent;
using Domain.Game.Days.DayEvents.WorkAnotherTeamDayEvent;

namespace Domain.Game.Days;

public class Day
{
	private readonly int analystsCount;
	private readonly bool anotherTeamAppeared;

	private readonly List<DayEvent> events;
	private readonly List<ExpectedEvent> expectedEvents;
	private readonly int programmersCount;
	private readonly bool shouldRelease;
	private readonly bool shouldUpdateSprintBacklog;
	private readonly int testersCount;

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
		this.anotherTeamAppeared = anotherTeamAppeared;
		this.shouldRelease = shouldRelease;
		this.shouldUpdateSprintBacklog = shouldUpdateSprintBacklog;
		Number = number;

		events = new List<DayEvent>();
		expectedEvents = new List<ExpectedEvent>();

		if (anotherTeamAppeared)
		{
			expectedEvents.Add(new ExpectedEvent(DayEventType.WorkAnotherTeam));
		}
		else
		{
			AppendFirstStepEvents();
		}
	}

	private void AppendFirstStepEvents()
	{
		expectedEvents.Add(new ExpectedEvent(DayEventType.UpdateTeamRoles));
		expectedEvents.Add(new ExpectedEvent(DayEventType.RollDice));
	}

	private int LastEventId => events.Max(t => t.Id);

	public long Number { get; }

	private ExpectedEvent GetExpectedEventOrThrow(DayEventType type, string errorMessage)
	{
		var expectedEvent = expectedEvents
			.Where(@event => !@event.Removed)
			.SingleOrDefault(@event => @event.EventType == type);

		return expectedEvent ?? throw new DayEventNotExpectedException(errorMessage);
	}

	public int RollDiceForAnotherTeam()
	{
		const string notExpectedMessage = "Roll dice for Petya is not expected this day or state";
		var workAnotherTeamEvent = GetExpectedEventOrThrow(DayEventType.WorkAnotherTeam, notExpectedMessage);

		var diceRoller = new DiceRoller(new Random());
		var diceNumber = diceRoller.RollDice();
		var diceScores = MapAnotherTeam(diceNumber);
		events.Add(new WorkAnotherTeamDayEvent(diceNumber, diceScores, LastEventId + 1));

		workAnotherTeamEvent.MarkRemoved();
		AppendFirstStepEvents();

		return diceScores;
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		const string notExpectedMessage = "Update team roles is not expected this state";
		GetExpectedEventOrThrow(DayEventType.UpdateTeamRoles, notExpectedMessage);

		events.Add(new UpdateTeamRolesDayEvent(from, to, LastEventId + 1));
	}

	public void RollDice()
	{
		const string notExpectedMessage = "Dice rolling is not expected";
		var updateTeamRolesEvent = GetExpectedEventOrThrow(DayEventType.UpdateTeamRoles, notExpectedMessage);
		var rollDiceEvent = GetExpectedEventOrThrow(DayEventType.RollDice, notExpectedMessage);

		var diceRoller = new DiceRoller(new Random());
		var swapRoleEvents = events
			.Where(@event => @event.Type == DayEventType.UpdateTeamRoles)
			.Where(@event => !@event.IsRemoved)
			.Cast<UpdateTeamRolesDayEvent>()
			.GroupBy(@event => @event.From)
			.ToDictionary(grouping => grouping.Key, grouping => grouping.Select(@event => @event.To).ToArray());

		var (analystsDiceNumber, analystsScores) = RollDiceForRole(
			analystsCount,
			TeamRole.Analyst,
			GetSwaps(TeamRole.Analyst),
			diceRoller);

		var (programmersDiceNumber, programmersScores) = RollDiceForRole(
			programmersCount,
			TeamRole.Programmer,
			GetSwaps(TeamRole.Programmer),
			diceRoller);

		var (testersDiceNumber, testersScores) = RollDiceForRole(
			testersCount,
			TeamRole.Tester,
			GetSwaps(TeamRole.Tester),
			diceRoller);

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

		TeamRole[] GetSwaps(TeamRole teamRole)
		{
			return swapRoleEvents.GetValueOrDefault(teamRole, Array.Empty<TeamRole>());
		}
	}

	private static (int[] diceNumber, int[] diceScores) RollDiceForRole(
		int roleSize,
		TeamRole role,
		TeamRole[] swaps,
		DiceRoller diceRoller)
	{
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

	#region mappings

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