using System.ComponentModel.DataAnnotations;
using Domain.DomainExceptions;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;

namespace Domain.Game.Days;

public class Day
{
	private readonly DayContext dayContext;

	private readonly int analystsNumber;
	private readonly int programmersNumber;
	private readonly int testersNumber;
	private readonly UpdateTeamRolesContainer updateTeamRolesContainer;

	public WorkAnotherTeamContainer? WorkAnotherTeamContainer { get; private set; }
	public RollDiceContainer? RollDiceContainer { get; private set; }
	public ReleaseTicketContainer? ReleaseTicketContainer { get; private set; }
	public UpdateSprintBacklogContainer? UpdateSprintBacklogContainer { get; private set; }
	public UpdateCfdContainer? UpdateCfdContainer { get; private set; }

	private Day()
	{
	}

	public Day(
		long teamSessionId,
		DayContext dayContext,
		int analystsNumber,
		int programmersNumber,
		int testersNumber)
	{
		Id = dayContext.DayId;
		TeamSessionId = teamSessionId;

		this.dayContext = dayContext;
		this.analystsNumber = analystsNumber;
		this.programmersNumber = programmersNumber;
		this.testersNumber = testersNumber;

		updateTeamRolesContainer = new UpdateTeamRolesContainer(dayContext.DayId);
	}

	public long TeamSessionId { get; }

	public int Id { get; }

	public byte[]? Timestamp { get; set; }

	public int RollDiceForAnotherTeam()
	{
		EnsureCanPostEvent(DayEventType.WorkAnotherTeam);

		var diceRoller = new DiceRoller(new Random());
		var diceNumber = diceRoller.RollDice();
		var diceScores = MapDiceNumberToScoreSettings.MapAnotherTeam(diceNumber);

		WorkAnotherTeamContainer = WorkAnotherTeamContainer.CreateInstance(dayContext, diceNumber, diceScores);

		return diceScores;
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		EnsureCanPostEvent(DayEventType.UpdateTeamRoles);
		EnsureCanUpdateTeamRoles(from);

		updateTeamRolesContainer.AddUpdate(dayContext, from, to);
	}

	public void RollDices()
	{
		EnsureCanPostEvent(DayEventType.RollDice);

		var diceRoller = new DiceRoller(new Random());
		var swapByRole = updateTeamRolesContainer.BuildTeamRolesUpdate();
		var (analystsDiceNumber, analystsScores) = RollDiceForRole(analystsNumber, TeamRole.Analyst);
		var (programmersDiceNumber, programmersScores) = RollDiceForRole(programmersNumber, TeamRole.Programmer);
		var (testersDiceNumber, testersScores) = RollDiceForRole(testersNumber, TeamRole.Tester);

		RollDiceContainer = RollDiceContainer.CreateInstance(
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
			var swaps = swapByRole.GetValueOrDefault(role, []);
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

		ReleaseTicketContainer = ReleaseTicketContainer.CreateInstance(dayContext, ticketIds);
	}

	public void UpdateSprintBacklog(string[] ticketIds)
	{
		EnsureCanPostEvent(DayEventType.UpdateSprintBacklog);

		UpdateSprintBacklogContainer = UpdateSprintBacklogContainer.CreateInstance(dayContext, ticketIds);
	}

	public void UpdateCfd(
		int released,
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		EnsureCanPostEvent(DayEventType.UpdateCfd);

		UpdateCfdContainer = UpdateCfdContainer.CreateInstance(
			dayContext,
			released,
			readyToDeploy,
			withTesters,
			withProgrammers,
			withAnalysts);
	}

	public void EndDay()
	{
		EnsureCanPostEvent(DayEventType.EndDay);

		dayContext.PostDayEvent(DayEventType.EndDay);
	}

	private void EnsureCanUpdateTeamRoles(TeamRole from)
	{
		var update = updateTeamRolesContainer.BuildTeamRolesUpdate();

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