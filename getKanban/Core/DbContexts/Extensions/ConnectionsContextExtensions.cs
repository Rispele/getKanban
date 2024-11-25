using Core.Entities;

namespace Core.DbContexts.Extensions;

public static class ConnectionsContextExtensions
{
	public static async Task<HubConnection?> GetCurrentConnection(
		this ConnectionsContext connectionsContext,
		Guid userId)
	{
		return connectionsContext.HubConnections.SingleOrDefault(x => x.UserId == userId);
	}
}