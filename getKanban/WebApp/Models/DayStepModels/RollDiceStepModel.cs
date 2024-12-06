using Core.Dtos.Containers.RollDice;

namespace WebApp.Models.DayStepModels;

public class RollDicesStepModel : StepModel
{
	public RollDiceContainerDto RollDiceContainerDto { get; set; }
}