using Domain.DomainExceptions;

namespace Domain.Game.Days.DayContainers;

public class FreezableDayContainer : DayContainer
{
	public bool Frozen { get; private set; }

	public void EnsureNotFrozen()
	{
		if (Frozen)
		{
			throw new DomainException("Cannot update frozen container");
		}
	}

	internal void Freeze()
	{
		Frozen = true;
	}
}