using Core.Configuration;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[EntityTypeConfiguration(typeof(HubConnectionEntityTypeConfiguration))]
public class HubConnection
{
	public Guid UserId { get; set; }

	public string LobbyId { get; set; } = null!;

	public string HubConnectionId { get; set; } = null!;
	
	[UsedImplicitly]
	public HubConnection()
	{
	}
}