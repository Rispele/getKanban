﻿using Domain;
using Domain.Game;
using Domain.Game.Days.DayEvents.DayContainers;
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
		var (context1, context2, session1, session2) = await SetupGameSessionInDifferentContexts();

		var (team1, team2) = (session1.Teams.Single(), session2.Teams.Single());
		team1.Name = "name1";
		team2.Name = "name2";

		await ShouldNotThrowOnSave(context1);
		await ShouldThrowOnSave<DbUpdateConcurrencyException>(context2);
	}

	[Test]
	public async Task Day_ConcurrentUpdate_ShouldThrowOnConflict()
	{
		var (context1, context2, session1, session2) = await SetupGameSessionInDifferentContexts();

		var (team1, team2) = (session1.Teams.Single(), session2.Teams.Single());
		team1.RollDices();
		team2.RollDices();

		await ShouldNotThrowOnSave(context1);
		await ShouldThrowOnSave<DbUpdateException>(context2);
	}

	[Test]
	public async Task UpdateCfdContainer_ConcurrentUpdate_ShouldThrowOnConflict()
	{
		var (context1, context2, session1, session2) = await SetupGameSessionInDifferentContexts(
			s => s.Teams.Single().RollDices(),
			s => s.Teams.Single().ReleaseTickets([]),
			s => s.Teams.Single().UpdateSprintBacklog([]));

		var (team1, team2) = (session1.Teams.Single(), session2.Teams.Single());
		team1.UpdateCfd(UpdateCfdContainerPatchType.ToDeploy, 10);
		team2.UpdateCfd(UpdateCfdContainerPatchType.ToDeploy, 5);

		await ShouldNotThrowOnSave(context1);
		await ShouldThrowOnSave<DbUpdateConcurrencyException>(context2);
	}

	private static async Task ShouldNotThrowOnSave(DbContext dbContext)
	{
		var save = () => dbContext.SaveChangesAsync();
		await save.Should().NotThrowAsync();
	}

	private static async Task ShouldThrowOnSave<TException>(DbContext dbContext)
		where TException : Exception
	{
		var save = () => dbContext.SaveChangesAsync();
		await save.Should().ThrowAsync<TException>();
	}

	private static async Task<GameSession> SetupSessionAsync(
		GameSessionsContext context,
		params Action<GameSession>[] actions)
	{
		var user = await context.AddAsync(new User("userName"));
		var session = await context.AddAsync(new GameSession(user.Entity, "name", 1));

		actions.ForEach(a => a(session.Entity));

		await context.SaveChangesAsync();
		return session.Entity;
	}

	private static async
		Task<(GameSessionsContext context1, GameSessionsContext context2, GameSession session1, GameSession session2)>
		SetupGameSessionInDifferentContexts(params Action<GameSession>[] actions)
	{
		var context1 = ConfigureDbContext();
		var context2 = ConfigureDbContext();
		var session1 = await SetupSessionAsync(context1, actions);
		var session2 = await context2.GameSessions
			.SingleOrDefaultAsync(s => s.Id == session1.Id);

		return (context1, context2, session1, session2!);
	}

	private static GameSessionsContext ConfigureDbContext()
	{
		var context = new GameSessionsContext();
		context.Database.EnsureCreated();
		return context;
	}
}