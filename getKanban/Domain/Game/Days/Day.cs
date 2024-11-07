using Domain.DomainExceptions;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;
using Domain.Game.Days.Scenarios;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days;

[EntityTypeConfiguration(typeof(DayEntityTypeConfiguration))]
public class Day
{
	private readonly List<AwaitedEvent> awaitedEvents = null!;
	private readonly Scenario scenario = null!;

	public int AnalystsNumber { get; }
	public int ProgrammersNumber { get; }
	public int TestersNumber { get; }

	private IEnumerable<AwaitedEvent> currentlyAwaitedEvents => awaitedEvents.Where(@event => !@event.Removed);
	public WorkAnotherTeamContainer? WorkAnotherTeamContainer { get; private set; }
	public UpdateTeamRolesContainer UpdateTeamRolesContainer { get; } = null!;
	public RollDiceContainer? RollDiceContainer { get; private set; }
	public ReleaseTicketContainer ReleaseTicketContainer { get; private set; }
	public UpdateSprintBacklogContainer UpdateSprintBacklogContainer { get; private set; }
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
		List<DayEventType> initiallyAwaitedEvents,
		int analystsNumber,
		int programmersNumber,
		int testersNumber)
	{
		Number = number;
		this.scenario = scenario;
		awaitedEvents = initiallyAwaitedEvents.Select(t => new AwaitedEvent(t)).ToList();

		AnalystsNumber = analystsNumber;
		ProgrammersNumber = programmersNumber;
		TestersNumber = testersNumber;

		UpdateTeamRolesContainer = new UpdateTeamRolesContainer();
		UpdateCfdContainer = new UpdateCfdContainer();
		ReleaseTicketContainer = new ReleaseTicketContainer();
		UpdateSprintBacklogContainer = new UpdateSprintBacklogContainer();
	}

	internal int RollDiceForAnotherTeam()
	{
		EnsureCanPostEvent(DayEventType.WorkAnotherTeam);

		var diceRoller = new DiceRoller(new Random());
		var diceNumber = diceRoller.RollDice();
		var diceScores = MapDiceNumberToScoreSettings.MapAnotherTeam(diceNumber);

		WorkAnotherTeamContainer = WorkAnotherTeamContainer.CreateInstance(this, diceNumber, diceScores);

		return diceScores;
	}

	internal void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		EnsureCanPostEvent(DayEventType.UpdateTeamRoles);
		EnsureCanUpdateTeamRoles(from);

		UpdateTeamRolesContainer.AddUpdate(from, to);
	}

	internal void RollDices()
	{
		EnsureCanPostEvent(DayEventType.RollDice);

		var diceRoller = new DiceRoller(new Random());
		var swapByRole = UpdateTeamRolesContainer?.BuildTeamRolesUpdate() ?? [];
		var (analystsDiceNumber, analystsScores) = RollDiceForRole(AnalystsNumber, TeamRole.Analyst);
		var (programmersDiceNumber, programmersScores) = RollDiceForRole(ProgrammersNumber, TeamRole.Programmer);
		var (testersDiceNumber, testersScores) = RollDiceForRole(TestersNumber, TeamRole.Tester);

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

	internal void ReleaseTickets(string[] ticketIds)
	{
		EnsureCanPostEvent(DayEventType.ReleaseTickets);
		
		ticketIds.ForEach(t => ReleaseTicketContainer.Update(t));
		UpdateCfdContainer.Update(UpdateCfdContainerPatchType.Released, ticketIds.Length);
	}
	
	internal void EndOfReleaseTickets()
	{
		EnsureCanPostEvent(DayEventType.EndOfReleaseTickets);
		
		ReleaseTicketContainer.Freeze();
		PostDayEvent(DayEventType.EndOfReleaseTickets, null);
	}

	internal void UpdateSprintBacklog(string[] ticketIds)
	{
		EnsureCanPostEvent(DayEventType.UpdateSprintBacklog);

		ticketIds.ForEach(t => UpdateSprintBacklogContainer.Update(t));
	}
	
	internal void EndOfUpdateSprintBacklog()
	{
		EnsureCanPostEvent(DayEventType.EndOfUpdateSprintBacklog);
		
		UpdateSprintBacklogContainer.Freeze();
		PostDayEvent(DayEventType.EndOfUpdateSprintBacklog, null);
	}

	internal void UpdateCfd(
		UpdateCfdContainerPatchType patchType,
		int value)
	{
		EnsureCanPostEvent(DayEventType.UpdateCfd);

		UpdateCfdContainer.Update(patchType, value);
	}

	internal void EndOfUpdateCfd()
	{
		EnsureCanPostEvent(DayEventType.EndOfUpdateCfd);

		UpdateCfdContainer.Freeze();

		PostDayEvent(DayEventType.EndOfUpdateCfd, null);
	}

	internal void EndDay()
	{
		EnsureCanPostEvent(DayEventType.EndDay);

		PostDayEvent(DayEventType.EndDay, null);
	}

	internal void PostDayEvent(DayEventType dayEventType, object? parameters)
	{
		var toAwait = scenario.GetNextAwaited(dayEventType, parameters);
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
			TeamRole.Analyst => AnalystsNumber,
			TeamRole.Programmer => ProgrammersNumber,
			TeamRole.Tester => TestersNumber,
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