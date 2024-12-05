using Domain.Game.Days.Commands;
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
			.For(DayCommandType.UpdateTeamRoles, b => b.AwaitCommands(DayCommandType.UpdateCfd))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, null);

		nextAwaited.toAdd.Should().Equal(DayCommandType.UpdateCfd);
	}

	[Test]
	public void SingleCommand_Reawait_ShouldReturn()
	{
		var scenario = ScenarioBuilder.Create()
			.For(DayCommandType.UpdateTeamRoles, b => b.ReAwaitCommand(DayCommandType.UpdateCfd))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, null);

		nextAwaited.toAdd.Should().Equal(DayCommandType.UpdateCfd);
		nextAwaited.toRemove.Should().Equal(DayCommandType.UpdateCfd);
	}

	[Test]
	public void SingleEvent_WithCondition_Match_ShouldReturn()
	{
		var scenario = ScenarioBuilder.Create()
			.WithScenarioService(new TestScenarioService())
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b.AwaitCommands(DayCommandType.UpdateCfd).WithValidationMethod("IsParamTrue"))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, true);

		nextAwaited.toAdd.Should().Equal(DayCommandType.UpdateCfd);
	}

	[Test]
	public void SingleEvent_WithCondition_NotMatch_ShouldReturnEmpty()
	{
		var scenario = ScenarioBuilder.Create()
			.WithScenarioService(new TestScenarioService())
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b.AwaitCommands(DayCommandType.UpdateCfd).WithValidationMethod("IsParamTrue"))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, false);

		nextAwaited.toAdd.Should().BeEmpty();
	}
	
	[Test]
	public void SingleEvent_WithSeveralConditions_MatchesFirst_ShouldReturnEmpty()
	{
		var scenario = ScenarioBuilder.Create()
			.WithScenarioService(new TestScenarioService())
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b
					.AwaitCommands(DayCommandType.EndDay)
					.WithValidationMethod("IsParamTrue"),
				b => b
					.AwaitCommands(DayCommandType.UpdateCfd)
					.WithValidationMethod("IsParamsTrue"))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, true);

		nextAwaited.toAdd.Should().ContainSingle(t => t == DayCommandType.EndDay);
	}
	
	[Test]
	public void SingleEvent_WithSeveralConditions_MatchesSecond_ShouldReturnEmpty()
	{
		var scenario = ScenarioBuilder.Create()
			.WithScenarioService(new TestScenarioService())
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b
					.AwaitCommands(DayCommandType.EndDay)
					.WithValidationMethod("IsParamTrue"),
				b => b
					.AwaitCommands(DayCommandType.UpdateCfd)
					.WithValidationMethod("IsParamsTrue"))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, true, true);

		nextAwaited.toAdd.Should().ContainSingle(t => t == DayCommandType.UpdateCfd);
	}
	
	[Test]
	public void SingleEvent_WithSeveralConditions_MatchesNone_ShouldReturnEmpty()
	{
		var scenario = ScenarioBuilder.Create()
			.WithScenarioService(new TestScenarioService())
			.For(
				DayCommandType.UpdateTeamRoles,
				b => b
					.AwaitCommands(DayCommandType.EndDay)
					.WithValidationMethod("IsParamTrue"),
				b => b
					.AwaitCommands(DayCommandType.UpdateCfd)
					.WithValidationMethod("IsParamsTrue"))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayCommandType.UpdateTeamRoles, false, true);

		nextAwaited.toAdd.Should().BeEmpty();
	}
}