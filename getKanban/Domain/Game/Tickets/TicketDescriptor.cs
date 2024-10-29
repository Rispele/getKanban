namespace Domain.Game.Tickets;

public class TicketDescriptor
{
	public TicketDescriptor(string id, int clientsProvides, int clientOffRate)
	{
		Id = id;
		ClientsProvides = clientsProvides;
		ClientOffRate = clientOffRate;
	}

	public string Id { get; }

	public int ClientsProvides { get; }

	public int ClientOffRate { get; }
}