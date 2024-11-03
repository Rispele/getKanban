using Domain.Game.Days.DayEvents;
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
			.For(DayEventType.UpdateTeamRoles, DayEventType.UpdateCfd)
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayEventType.UpdateTeamRoles, null);

		nextAwaited.Should().Equal(DayEventType.UpdateCfd);
	}

	[Test]
	public void SingleEvent_WithCondition_Match_ShouldReturn()
	{
		var scenario = ScenarioBuilder.Create()
			.For(
				DayEventType.UpdateTeamRoles,
				b => b.ForEventType(DayEventType.UpdateCfd).WithCondition("param", true))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayEventType.UpdateTeamRoles, new { param = true });

		nextAwaited.Should().Equal(DayEventType.UpdateCfd);
	}

	[Test]
	public void SingleEvent_WithCondition_NotMatch_ShouldReturnEmpty()
	{
		var scenario = ScenarioBuilder.Create()
			.For(
				DayEventType.UpdateTeamRoles,
				b => b.ForEventType(DayEventType.UpdateCfd).WithCondition("param", true))
			.Build();

		var nextAwaited = scenario.GetNextAwaited(DayEventType.UpdateTeamRoles, new { param = false });

		nextAwaited.Should().BeEmpty();
	}

	[Test]
	public void SingleEvent_WithSeveralConditions_Match_ShouldReturn()
	{
		var scenario = ScenarioBuilder.Create()
			.For(
				DayEventType.UpdateTeamRoles,
				b => b
					.ForEventType(DayEventType.UpdateCfd)
					.WithCondition("param1", true)
					.WithCondition("param2", true))
			.Build();

		var nextAwaited = scenario
			.GetNextAwaited(DayEventType.UpdateTeamRoles, new { param1 = true, param2 = true });

		nextAwaited.Should().Equal(DayEventType.UpdateCfd);
	}

	[Test]
	public void SingleEvent_WithSeveralConditions_NotMatchOne_ShouldReturnEmpty()
	{
		var scenario = ScenarioBuilder.Create()
			.For(
				DayEventType.UpdateTeamRoles,
				b => b
					.ForEventType(DayEventType.UpdateCfd)
					.WithCondition("param1", true)
					.WithCondition("param2", true))
			.Build();

		var nextAwaited = scenario
			.GetNextAwaited(DayEventType.UpdateTeamRoles, new { param1 = true, param2 = false });

		nextAwaited.Should().BeEmpty();
	}
}