using Core.DbContexts;
using Core.DbContexts.Helpers;
using Domain;
using Domain.DbContexts;
using Domain.Game;
using Domain.Game.Days.Commands;
using Domain.Game.Days.Configurations;
using Domain.Game.Days.DayContainers;
using Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NUnit.Framework;

namespace Tests.DbContexts.SaveTests;

[TestFixture]
public class UpdateSpringBacklogSaveTicketsTests
{
	[Test]
	public async Task UpdateTeamRoles_ConcurrentUpdate_ShouldThrowOnConflict()
	{
		var context = ConfigureDbContext();
		await context.Database.EnsureCreatedAsync();
		var session = await SetupGameSession(
			context,
			session => session.Teams.Single().ExecuteCommand(new RollDiceCommand()));
		
		var command = new UpdateSprintBacklogCommand
		{
			TicketIds = ["S12"],
			Remove = false
		};
		session.Teams.Single().ExecuteCommand(command);
		await context.TrySaveChangesAsync();
		
		var context2 = ConfigureDbContext();
		var sessionFromDb = context2.Find<GameSession>(session.Id);

		sessionFromDb!.Teams.Single().CurrentDay!.UpdateSprintBacklogContainer.TicketIds.Should().BeEquivalentTo("S12");
	}

	private static async Task<GameSession> SetupGameSession(DomainContext context, params Action<GameSession>[] actions)
	{
		var user = await context.AddAsync(new User("userName"));
		var session = await context.AddAsync(new GameSession(user.Entity, "name", 1));

		session.Entity.Start();
		actions.ForEach(a => a(session.Entity));

		await context.TrySaveChangesAsync();
		return session.Entity;
	}

	private static DomainContext ConfigureDbContext()
	{
		var context = TestDomainContextProvider.Get();
		context.Database.EnsureCreated();
		return context;
	}
}