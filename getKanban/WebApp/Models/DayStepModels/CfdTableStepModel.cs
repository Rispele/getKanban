namespace WebApp.Models.DayStepModels;

public class CfdTableStepModel : StepModel 
{
	public int? Released { get; set; }
	public int? ToDeploy { get; set; }
	public int? WithTesters { get; set; }
	public int? WithProgrammers { get; set; }
	public int? WithAnalysts { get; set; }
}