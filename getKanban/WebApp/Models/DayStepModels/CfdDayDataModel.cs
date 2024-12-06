using JetBrains.Annotations;

namespace WebApp.Models.DayStepModels;

public class CfdDayDataModel
{
	public int Released { get; [UsedImplicitly] set; }
	public int ToDeploy { get; [UsedImplicitly] set; }
	public int WithTesters { get; [UsedImplicitly] set; }
	public int WithProgrammers { get; [UsedImplicitly] set; }
	public int WithAnalysts { get; [UsedImplicitly] set; }
}