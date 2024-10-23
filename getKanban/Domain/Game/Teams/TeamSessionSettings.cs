using Domain.DomainExceptions;

namespace Domain.Game.Teams;

public class TeamSessionSettings
{
	public TeamSessionSettings()
	{
		AutoTestsAppeared = false;
		AutoReleaseAppeared = false;
	}

	public bool AutoTestsAppeared { get; private set; }

	public bool AutoReleaseAppeared { get; private set; }

	public void ReleaseAutoTests()
	{
		if (AutoTestsAppeared)
		{
			throw new DomainException("Could not release auto-tests several times");
		}

		AutoTestsAppeared = true;
	}

	public void ReleaseAutoRelease()
	{
		if (AutoReleaseAppeared)
		{
			throw new DomainException("Could not release auto-release several times");
		}

		AutoReleaseAppeared = true;
	}
}