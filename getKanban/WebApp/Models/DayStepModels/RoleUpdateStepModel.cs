using Core.Dtos.Containers;

namespace WebApp.Models.DayStepModels;

public class RoleUpdateStepModel : StepModel
{
	public List<TeamMemberDto> TeamMemberDtos { get; set; }
	
	public bool ShouldLockTesters { get; set; }
}