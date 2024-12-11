namespace WebApp.Models.DayStepModels;

public class GameResultModel
{
	public Guid SessionId { get; set; }
	public Guid RequesterTeamId { get; set; }
	public List<TeamResultModel> TeamResultModels { get; set; }
}