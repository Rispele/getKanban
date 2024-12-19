using Domain.ExtendedTracker;
using Domain.Game;
using Domain.Game.Teams;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Domain.DbContexts;

public sealed class DomainContext : DbContext
{
	public DbSet<GameSession> GameSessions { get; set; }
	public DbSet<Team> Teams { get; set; }
	public DbSet<User> Users { get; set; }

	public DomainContext(DbContextOptions<DomainContext> options)
		: base(options)
	{
		var registry = new ExtendedTrackerRegistry(typeof(DomainContext).Assembly);
		var extendedTrackerService = new ExtendedTrackerService(registry);

		ChangeTracker.StateChanged += (_, e) => extendedTrackerService.SetModifiedIfUpdated(e);
		ChangeTracker.Tracked += (_, e) => extendedTrackerService.SetModifiedIfUpdated(e);
	}
}