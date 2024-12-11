using JetBrains.Annotations;

namespace WebApp.Models.DayStepModels;

public class CfdDayDataModel
{
	public string PatchType { get; [UsedImplicitly] set; }
	public int Value { get; [UsedImplicitly] set; }
}