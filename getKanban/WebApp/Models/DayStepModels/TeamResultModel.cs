namespace WebApp.Models.DayStepModels;

public class TeamResultModel
{
	public Guid TeamId { get; set; }
	public string TeamName { get; set; }
	public int ClientsCount { get; set; }
	public int MoneyCount { get; set; }
	public bool IsTeamSessionEnded { get; set; }
}