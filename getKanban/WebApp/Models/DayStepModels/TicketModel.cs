using JetBrains.Annotations;

namespace WebApp.Models.DayStepModels;

public class TicketModel
{
	public string TicketId { get; [UsedImplicitly] set; }
	public bool Remove { get; [UsedImplicitly] set; }
}