namespace Domain.DomainExceptions;

public class DayEventNotAwaitedException : DomainException
{
	public DayEventNotAwaitedException(string message)
		: base(message)
	{
	}
}