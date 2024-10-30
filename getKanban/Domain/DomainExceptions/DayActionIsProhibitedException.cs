namespace Domain.DomainExceptions;

public class DayActionIsProhibitedException : DomainException
{
	public DayActionIsProhibitedException(string message)
		: base(message)
	{
	}
}