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

	private readonly int analystsNumber;
	private readonly int programmersNumber;
	private readonly int testersNumber;

	public Day(
		long teamSessionId,
		DayContext dayContext,
		int analystsNumber,
		int programmersNumber,
		int testersNumber)
	{
		TeamSessionId = teamSessionId;

		this.dayContext = dayContext;
		this.analystsNumber = analystsNumber;
		this.programmersNumber = programmersNumber;
		this.testersNumber = testersNumber;
	}

	public long TeamSessionId { get; }

	public Lazy<Dictionary<TeamRole, TeamRole[]>> TeamRolesUpdate => dayContext.TeamRolesUpdate;

	public Lazy<IReadOnlyList<string>> ReleasedTickets => dayContext.ReleasedTickets;

	public Lazy<IReadOnlyList<string>> TakenTickets => dayContext.TakenTickets;

	public Lazy<int> AnotherTeamScores => dayContext.AnotherTeamScores;

	public int RollDiceForAnotherTeam()
	{
		EnsureCanPostEvent(DayEventType.WorkAnotherTeam);

		var diceRoller = new DiceRoller(new Random());
		var diceNumber = diceRoller.RollDice();
		var diceScores = MapDiceNumberToScoreSettings.MapAnotherTeam(diceNumber);

		WorkAnotherTeamDayEvent.CreateInstance(dayContext, diceNumber, diceScores);

		return diceScores;
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		EnsureCanPostEvent(DayEventType.UpdateTeamRoles);
		EnsureCanUpdateTeamRoles(from);

		UpdateTeamRolesDayEvent.CreateInstance(dayContext, from, to);
	}

	public void RollDices()
	{
		EnsureCanPostEvent(DayEventType.RollDice);

		var diceRoller = new DiceRoller(new Random());
		var (analystsDiceNumber, analystsScores) = RollDiceForRole(analystsNumber, TeamRole.Analyst);
		var (programmersDiceNumber, programmersScores) = RollDiceForRole(programmersNumber, TeamRole.Programmer);
		var (testersDiceNumber, testersScores) = RollDiceForRole(testersNumber, TeamRole.Tester);

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
				diceScores[i] = MapDiceNumberToScoreSettings.MapByRole(role, asRole, diceNumber[i]);
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

		var totalReleased = released + dayContext.ReleasedTickets.Value.Count;

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

	private void EnsureCanUpdateTeamRoles(TeamRole from)
	{
		var update = dayContext.TeamRolesUpdate.Value;

		if (!update.TryGetValue(from, out var updates))
		{
			return;
		}

		var limit = from switch
		{
			TeamRole.Analyst => analystsNumber,
			TeamRole.Programmer => programmersNumber,
			TeamRole.Tester => testersNumber,
			_ => throw new ArgumentOutOfRangeException()
		};

		if (updates.Length + 1 > limit)
		{
			throw new DayActionIsProhibitedException(
				$"{from} role updates count can't be more than members count{limit}");
		}
	}

	private void EnsureCanPostEvent(DayEventType eventType)
	{
		if (dayContext.CanPostEvent(eventType))
		{
			return;
		}

		throw new DayEventNotAwaitedException($"{eventType} is not awaited");
	}
}