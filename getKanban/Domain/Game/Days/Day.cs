using Domain.DomainExceptions;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days;

[EntityTypeConfiguration(typeof(DayEntityTypeConfiguration))]
public class Day
{
	private readonly List<AwaitedEvent> awaitedEvents = null!;
	private readonly Dictionary<DayEventType, List<DayEventType>> scenario = null!;

	private readonly int analystsNumber;
	private readonly int programmersNumber;
	private readonly int testersNumber;

	private IEnumerable<AwaitedEvent> currentlyAwaitedEvents => awaitedEvents.Where(@event => !@event.Removed);

	public UpdateTeamRolesContainer UpdateTeamRolesContainer { get; private set; }
	public WorkAnotherTeamContainer? WorkAnotherTeamContainer { get; private set; }
	public RollDiceContainer? RollDiceContainer { get; private set; }
	public ReleaseTicketContainer? ReleaseTicketContainer { get; private set; }
	public UpdateSprintBacklogContainer? UpdateSprintBacklogContainer { get; private set; }
	public UpdateCfdContainer UpdateCfdContainer { get; private set; }

	public long TeamSessionId { get; }

	public long Id { get; }

	public byte[]? Timestamp { get; set; }

	[UsedImplicitly]
	private Day()
	{
	}

	public Day(
		long teamSessionId,
		Dictionary<DayEventType, List<DayEventType>> scenario,
		List<DayEventType> initiallyAwaitedEvents,
		int analystsNumber,
		int programmersNumber,
		int testersNumber)
	{
		TeamSessionId = teamSessionId;

		this.scenario = scenario;
		awaitedEvents = initiallyAwaitedEvents.Select(t => new AwaitedEvent(t)).ToList();

		this.analystsNumber = analystsNumber;
		this.programmersNumber = programmersNumber;
		this.testersNumber = testersNumber;

		UpdateTeamRolesContainer = new UpdateTeamRolesContainer();
		// UpdateCfdContainer = new UpdateCfdContainer();
	}

	public int RollDiceForAnotherTeam()
	{
		EnsureCanPostEvent(DayEventType.WorkAnotherTeam);

		var diceRoller = new DiceRoller(new Random());
		var diceNumber = diceRoller.RollDice();
		var diceScores = MapDiceNumberToScoreSettings.MapAnotherTeam(diceNumber);

		WorkAnotherTeamContainer = WorkAnotherTeamContainer.CreateInstance(this, diceNumber, diceScores);

		return diceScores;
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		EnsureCanPostEvent(DayEventType.UpdateTeamRoles);
		EnsureCanUpdateTeamRoles(from);

		UpdateTeamRolesContainer ??= new UpdateTeamRolesContainer();

		UpdateTeamRolesContainer.AddUpdate(this, from, to);
	}

	public void RollDices()
	{
		EnsureCanPostEvent(DayEventType.RollDice);

		var diceRoller = new DiceRoller(new Random());
		var swapByRole = UpdateTeamRolesContainer?.BuildTeamRolesUpdate() ?? [];
		var (analystsDiceNumber, analystsScores) = RollDiceForRole(analystsNumber, TeamRole.Analyst);
		var (programmersDiceNumber, programmersScores) = RollDiceForRole(programmersNumber, TeamRole.Programmer);
		var (testersDiceNumber, testersScores) = RollDiceForRole(testersNumber, TeamRole.Tester);

		RollDiceContainer = RollDiceContainer.CreateInstance(
			this,
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

		ReleaseTicketContainer = ReleaseTicketContainer.CreateInstance(this, ticketIds);
	}

	public void UpdateSprintBacklog(string[] ticketIds)
	{
		EnsureCanPostEvent(DayEventType.UpdateSprintBacklog);

		UpdateSprintBacklogContainer = UpdateSprintBacklogContainer.CreateInstance(this, ticketIds);
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
			this,
			released,
			readyToDeploy,
			withTesters,
			withProgrammers,
			withAnalysts);
	}

	public void EndOfUpdateCfd()
	{
		EnsureCanPostEvent(DayEventType.EndOfUpdateCfd);

		PostDayEvent(DayEventType.EndOfUpdateCfd);
	}

	public void EndDay()
	{
		EnsureCanPostEvent(DayEventType.EndDay);

		PostDayEvent(DayEventType.EndDay);
	}

	public void PostDayEvent(DayEventType dayEventType)
	{
		var toAwait = scenario[dayEventType];
		currentlyAwaitedEvents.ForEach(e => e.MarkRemoved());
		awaitedEvents.AddRange(toAwait.Select(e => new AwaitedEvent(e)));
	}

	private void EnsureCanUpdateTeamRoles(TeamRole from)
	{
		var update = UpdateTeamRolesContainer?.BuildTeamRolesUpdate() ?? [];

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
		if (CanPostEvent(eventType))
		{
			return;
		}

		throw new DayEventNotAwaitedException($"{eventType} is not awaited");
	}

	private bool CanPostEvent(DayEventType eventType)
	{
		return currentlyAwaitedEvents.Any(e => e.EventType == eventType);
	}
}