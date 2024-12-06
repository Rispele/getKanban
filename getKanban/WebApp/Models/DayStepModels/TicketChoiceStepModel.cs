using Domain.Game;

namespace WebApp.Models.DayStepModels;

public class TicketChoiceStepModel : StepModel
{
	public string PageType { get; set; }

	public List<Ticket> TicketIds { get; set; }
}