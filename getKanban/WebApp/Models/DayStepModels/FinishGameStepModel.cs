namespace WebApp.Models.DayStepModels;

public class FinishGameStepModel : StepModel
{
	public string EndDayEventMessage { get; set; }
	public bool ShouldFinishGame { get; set; }
	public bool IsLastDay { get; set; }
}