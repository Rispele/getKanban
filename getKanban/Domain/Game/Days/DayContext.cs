using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.ReleaseTicketDayEvent;
using Domain.Game.Days.DayEvents.UpdateSprintBacklogDayEvent;
using Domain.Game.Days.DayEvents.UpdateTeamRolesDayEvent;
using Domain.Game.Days.DayEvents.WorkAnotherTeamDayEvent;

namespace Domain.Game.Days;

public class DayContext
{
	private readonly List<AwaitedEvent> awaitedEvents = null!;
	private readonly List<DayEvent> events = null!;
	private readonly Dictionary<DayEventType, List<DayEventType>> scenario = null!;

	private IEnumerable<AwaitedEvent> CurrentlyAwaitedEvents => awaitedEvents.Where(@event => !@event.Removed);
	private IEnumerable<DayEvent> ExistingEvents => events.Where(@event => !@event.Removed);

	private DayContext()
	{
		TeamRolesUpdate = new Lazy<Dictionary<TeamRole, TeamRole[]>>(BuildTeamRoleUpdate);
		ReleasedTickets = new Lazy<IReadOnlyList<string>>(BuildReleasedTickets);
		TakenTickets = new Lazy<IReadOnlyList<string>>(BuildTakenTickets);
		AnotherTeamScores = new Lazy<int>(BuildAnotherTeamScores);
	}

	public DayContext(
		int dayId,
		Dictionary<DayEventType, List<DayEventType>> dayScenario,
		params DayEventType[] initialAwaitedEvents)
		: this()
	{
		DayId = dayId;

		scenario = dayScenario;
		awaitedEvents = initialAwaitedEvents.Select(e => new AwaitedEvent(e)).ToList();
		events = [];
	}

	public int DayId { get; }

	public int NextEventId => events.Max(t => t.Id) + 1;

	public Lazy<Dictionary<TeamRole, TeamRole[]>> TeamRolesUpdate { get; }

	public Lazy<IReadOnlyList<string>> ReleasedTickets { get; }

	public Lazy<IReadOnlyList<string>> TakenTickets { get; }

	public Lazy<int> AnotherTeamScores { get; }

	public void PostDayEvent(DayEvent dayEvent)
	{
		events.Add(dayEvent);

		var eventType = dayEvent.Type;
		var toAwait = scenario[eventType];

		ExistingEvents.ForEach(e => e.MarkRemoved());
		awaitedEvents.AddRange(toAwait.Select(e => new AwaitedEvent(e)));
	}

	public bool CanPostEvent(DayEventType eventType)
	{
		return CurrentlyAwaitedEvents.Any(e => e.EventType == eventType);
	}

	private Dictionary<TeamRole, TeamRole[]> BuildTeamRoleUpdate()
	{
		return ExistingEvents
			.Where(@event => @event.Type == DayEventType.UpdateTeamRoles)
			.Where(@event => !@event.Removed)
			.Cast<UpdateTeamRolesDayEvent>()
			.GroupBy(@event => @event.From)
			.ToDictionary(grouping => grouping.Key, grouping => grouping.Select(@event => @event.To).ToArray());
	}

	private IReadOnlyList<string> BuildReleasedTickets()
	{
		var @event = ExistingEvents.SingleOrDefault(@event => @event.Type == DayEventType.ReleaseTickets);
		return ((ReleaseTicketDayEvent?)@event)?.TicketIds ?? [];
	}

	private IReadOnlyList<string> BuildTakenTickets()
	{
		var @event = ExistingEvents.SingleOrDefault(@event => @event.Type == DayEventType.UpdateSprintBacklog);
		return ((UpdateSprintBacklogDayEvent?)@event)?.TicketIds ?? [];
	}

	private int BuildAnotherTeamScores()
	{
		var @event = ExistingEvents.SingleOrDefault(@event => @event.Type == DayEventType.WorkAnotherTeam);
		return ((WorkAnotherTeamDayEvent?)@event)?.ScoresNumber ?? 0;
	}
}