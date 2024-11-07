namespace Domain.Game.Days.DayContainers;

public class FreezableDayContainer : DayContainer
{
	public bool Frozen { get; private set; }

	internal void Freeze()
	{
		Frozen = true;
	}
}