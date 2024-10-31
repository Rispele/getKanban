using Domain.Game.Days.DayEvents;

namespace Domain.Game.Days;

public class DayContext
{
	private readonly List<AwaitedEvent> awaitedEvents = null!;
	private readonly Dictionary<DayEventType, List<DayEventType>> scenario = null!;

	private IEnumerable<AwaitedEvent> CurrentlyAwaitedEvents => awaitedEvents.Where(@event => !@event.Removed);

	private DayContext()
	{
	}

	public DayContext(
		int dayId,
		Dictionary<DayEventType, List<DayEventType>> dayScenario,
		params DayEventType[] initialAwaitedEvents)
	{
		DayId = dayId;

		scenario = dayScenario;
		awaitedEvents = initialAwaitedEvents.Select(e => new AwaitedEvent(dayId, e)).ToList();
	}

	public int DayId { get; }

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