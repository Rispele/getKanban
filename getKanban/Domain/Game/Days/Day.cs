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
	private readonly DayContext dayContext;

	private readonly int analystsCount;
	private readonly int programmersCount;
	private readonly int testersCount;

	public Day(
		DayContext dayContext,
		int analystsCount,
		int programmersCount,
		int testersCount)
	{
		this.dayContext = dayContext;
		this.analystsCount = analystsCount;
		this.programmersCount = programmersCount;
		this.testersCount = testersCount;
	}

	public int RollDiceForAnotherTeam()
	{
		EnsureCanPostEvent(DayEventType.WorkAnotherTeam);

		var diceRoller = new DiceRoller(new Random());
		var diceNumber = diceRoller.RollDice();
		var diceScores = MapAnotherTeam(diceNumber);

		WorkAnotherTeamDayEvent.CreateInstance(dayContext, diceNumber, diceScores);

		return diceScores;
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		EnsureCanPostEvent(DayEventType.UpdateTeamRoles);

		UpdateTeamRolesDayEvent.CreateInstance(dayContext, from, to);
	}

	public void RollDices()
	{
		EnsureCanPostEvent(DayEventType.RollDice);

		var diceRoller = new DiceRoller(new Random());
		var (analystsDiceNumber, analystsScores) = RollDiceForRole(analystsCount, TeamRole.Analyst);
		var (programmersDiceNumber, programmersScores) = RollDiceForRole(programmersCount, TeamRole.Programmer);
		var (testersDiceNumber, testersScores) = RollDiceForRole(testersCount, TeamRole.Tester);

		RollDiceDayEvent.CreateInstance(
			dayContext,
			analystsDiceNumber,
			programmersDiceNumber,
			testersDiceNumber,
			analystsScores,
			programmersScores,
			testersScores);
		return;

		(int[] diceNumber, int[] diceScores) RollDiceForRole(int roleSize, TeamRole role)
		{
			var swaps = dayContext.TeamRolesUpdate.Value.GetValueOrDefault(role, []);
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
		EnsureCanPostEvent(DayEventType.ReleaseTickets);

		ReleaseTicketDayEvent.CreateInstance(dayContext, ticketIds);
	}

	public void UpdateSprintBacklog(string[] ticketIds)
	{
		EnsureCanPostEvent(DayEventType.UpdateSprintBacklog);

		UpdateSprintBacklogDayEvent.CreateInstance(dayContext, ticketIds);
	}

	public void UpdateCfd(
		int released,
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		EnsureCanPostEvent(DayEventType.UpdateCfd);

		var totalReleased = released + dayContext.ReleasedThisDay.Value;

		UpdateCfdDayEvent.CreateInstance(
			dayContext,
			totalReleased,
			readyToDeploy,
			withTesters,
			withProgrammers,
			withAnalysts);
	}

	public void EndDay()
	{
		EnsureCanPostEvent(DayEventType.EndDay);

		EndDayDayEvent.CreateInstance(dayContext);
	}

	private void EnsureCanPostEvent(DayEventType eventType)
	{
		if (dayContext.CanPostEvent(eventType))
		{
			return;
		}

		throw new DayEventNotAwaitedException($"{eventType} is not awaited");
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