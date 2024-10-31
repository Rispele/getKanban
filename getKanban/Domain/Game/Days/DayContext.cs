using Domain.Game.Days.DayEvents;

namespace Domain.Game.Days;

public class DayContext
{
	private readonly List<AwaitedEvent> awaitedEvents = null!;
	private readonly Dictionary<DayEventType, List<DayEventType>> scenario = null!;

	private IEnumerable<AwaitedEvent> CurrentlyAwaitedEvents => awaitedEvents.Where(@event => !@event.Removed);

	public int DayId { get; }

	private DayContext()
	{
	}

	public DayContext(
		Dictionary<DayEventType, List<DayEventType>> dayScenario,
		params DayEventType[] initialAwaitedEvents)
	{
		scenario = dayScenario;
		awaitedEvents = initialAwaitedEvents.Select(e => new AwaitedEvent(DayId, e)).ToList();
	}

	public void PostDayEvent(DayEventType dayEventType)
	{
		var toAwait = scenario[dayEventType];
		CurrentlyAwaitedEvents.ForEach(e => e.MarkRemoved());
		awaitedEvents.AddRange(toAwait.Select(e => new AwaitedEvent(DayId, e)));
	}

	public bool CanPostEvent(DayEventType eventType)
	{
		return CurrentlyAwaitedEvents.Any(e => e.EventType == eventType);
	}
}