namespace Domain.DomainExceptions;

public class DayEventNotExpectedException : DomainException
{
	public DayEventNotExpectedException(string message) : base(message)
	{
	}
}