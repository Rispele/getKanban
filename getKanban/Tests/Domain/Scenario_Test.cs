using Domain.Game.Days;
using Domain.Game.Days.Scenarios;
using FluentAssertions;
using NUnit.Framework;

namespace Tests.Domain;

[TestFixture]
public class Scenario_Test
{
	[Test]
	public void SingleEvent_NoCondition_ShouldReturn()
	{
		var scenario = ScenarioBuilder.Create()
			.For(DayCommandType.UpdateTeamRoles, b=> b.AwaitCommands(DayCommandType.UpdateCfd))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, null);

		nextAwaited.toAdd.Should().Equal(DayCommandType.UpdateCfd);
	}
	
	[Test]
	public void SingleCommand_Reawait_ShouldReturn()
	{
		var scenario = ScenarioBuilder.Create()
			.For(DayCommandType.UpdateTeamRoles, b=> b.ReAwaitCommand(DayCommandType.UpdateCfd))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, null);

		nextAwaited.toAdd.Should().Equal(DayCommandType.UpdateCfd);
		nextAwaited.toRemove.Should().Equal(DayCommandType.UpdateCfd);
	}

	[Test]
	public void SingleEvent_WithCondition_Match_ShouldReturn()
	{
		var scenario = ScenarioBuilder.Create()
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b.AwaitCommands(DayCommandType.UpdateCfd).WithCondition("param", true))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, new { param = true });

		nextAwaited.toAdd.Should().Equal(DayCommandType.UpdateCfd);
	}

	[Test]
	public void SingleEvent_WithCondition_NotMatch_ShouldReturnEmpty()
	{
		var scenario = ScenarioBuilder.Create()
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b.AwaitCommands(DayCommandType.UpdateCfd).WithCondition("param", true))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, new { param = false });

		nextAwaited.toAdd.Should().BeEmpty();
	}

	[Test]
	public void SingleEvent_WithSeveralConditions_Match_ShouldReturn()
	{
		var scenario = ScenarioBuilder.Create()
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b
					.AwaitCommands(DayCommandType.UpdateCfd)
					.WithCondition("param1", true)
					.WithCondition("param2", true))
			.Build();

		var nextAwaited = scenario
			.GetNextAwaited(DayCommandType.UpdateTeamRoles, new { param1 = true, param2 = true });

		nextAwaited.toAdd.Should().Equal(DayCommandType.UpdateCfd);
	}

	[Test]
	public void SingleEvent_WithSeveralConditions_NotMatchOne_ShouldReturnEmpty()
	{
		var scenario = ScenarioBuilder.Create()
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b
					.AwaitCommands(DayCommandType.UpdateCfd)
					.WithCondition("param1", true)
					.WithCondition("param2", true))
			.Build();

		var nextAwaited = scenario
			.GetNextAwaited(DayCommandType.UpdateTeamRoles, new { param1 = true, param2 = false });

		nextAwaited.toAdd.Should().BeEmpty();
	}
}