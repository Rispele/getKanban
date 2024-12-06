namespace Core.Dtos.Statistics;

public class CfdStatisticDto
{
	public int WithAnalysts { get; }
	public int WithProgrammers { get; }
	public int WithTesters { get; }
	public int ToDeploy { get; }
	public int Released { get; }

	private CfdStatisticDto(
		int withAnalysts,
		int withProgrammers,
		int withTesters,
		int toDeploy,
		int released)
	{
		WithAnalysts = withAnalysts;
		WithProgrammers = withProgrammers;
		WithTesters = withTesters;
		ToDeploy = toDeploy;
		Released = released;
	}

	public static CfdStatisticDto Create(
		int withAnalysts,
		int withProgrammers,
		int withTesters,
		int toDeploy,
		int released)
	{
		return new CfdStatisticDto(
			withAnalysts,
			withProgrammers,
			withTesters,
			toDeploy,
			released);
	}
}