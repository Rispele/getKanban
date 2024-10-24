namespace Domain.Game.Tickets;

public class TicketDescriptor
{
	public string Id { get; }
	
	public int ClientsProvides { get; }
	
	public int ClientOffRate { get; }

	public TicketDescriptor(string id, int clientsProvides, int clientOffRate)
	{
		Id = id;
		ClientsProvides = clientsProvides;
		ClientOffRate = clientOffRate;
	}
}