namespace Core.Dtos;

public class UpdateCfdContainerDto : DayContainerDto
{
	public int? Released { get; init; }
	public int? ToDeploy { get; init; }
	public int? WithTesters { get; init; }
	public int? WithProgrammers { get; init; }
	public int? WithAnalysts { get; init; }
}