using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.DbContexts.Extensions;

public static class ConnectionsContextExtensions
{
	public static async Task<HubConnection?> FindCurrentConnection(
		this ConnectionsContext connectionsContext,
		Guid userId)
	{
		return await connectionsContext.HubConnections.SingleOrDefaultAsync(x => x.UserId == userId);
	}
}