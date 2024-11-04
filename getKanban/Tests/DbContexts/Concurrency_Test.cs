using Domain.Game;
using Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Web.DbContexts;

namespace Tests.DbContexts;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class Concurrency_Test
{
	[Test]
	public async Task Team_ConcurrentUpdate_ShouldThrowOnConflict()
	{
		var context1 = ConfigureDbContext();
		var context2 = ConfigureDbContext();
		var session1 = await SetupSessionAsync(context1);
		var session2 = await context2.GameSessions
			.SingleOrDefaultAsync(s => s.Id == session1.Id);

		var (team1, team2) = (session1.Teams.Single(), session2.Teams.Single());
		team1.Name = "name1";
		team2.Name = "name2";

		var save1 = () => context1.SaveChangesAsync();
		var save2 = () => context2.SaveChangesAsync();

		await save1.Should().NotThrowAsync();
		await save2.Should().ThrowAsync<DbUpdateConcurrencyException>();
	}

	[Test]
	public async Task Day_ConcurrentUpdate_ShouldThrowOnConflict()
	{
		var context1 = ConfigureDbContext();
		var context2 = ConfigureDbContext();
		var session1 = await SetupSessionAsync(context1);
		var session2 = await context2.GameSessions
			.SingleOrDefaultAsync(s => s.Id == session1.Id);

		var (team1, team2) = (session1.Teams.Single(), session2.Teams.Single());
		team1.RollDices();
		team2.RollDices();

		var save1 = () => context1.SaveChangesAsync();
		var save2 = () => context2.SaveChangesAsync();

		await save1.Should().NotThrowAsync();
		await save2.Should().ThrowAsync<DbUpdateException>();
	}

	private static async Task<GameSession> SetupSessionAsync(GameSessionsContext context)
	{
		var user = await context.AddAsync(new User("userName"));
		var session = await context.AddAsync(new GameSession(user.Entity, "name", 1));
		await context.SaveChangesAsync();
		return session.Entity;
	}

	private static GameSessionsContext ConfigureDbContext()
	{
		var context = new GameSessionsContext();
		context.Database.EnsureCreated();
		return context;
	}
}