namespace WebApp.Models.DayStepModels;

public class StepModel
{
	public Guid GameSessionId { get; set; }
	public Guid TeamId { get; set; }
	public string TeamName { get; set; }
	public int DayNumber { get; set; }
}