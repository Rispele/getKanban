using Domain.Game.Days;
using Domain.Game.Days.Commands;
using Domain.Game.Days.Scenarios;
using Domain.Serializers;
using FluentAssertions;
using NUnit.Framework;

namespace Tests.Domain;

[TestFixture]
public class Scenario_Test
{
	[Test]
	public void pp()
	{
		DayCommand t = new ReleaseTicketsCommand()
		{
			TicketIds = ["123"]
		};
	
		Console.WriteLine(t.ToJson());
		Console.WriteLine(t.ToJson().FromJson<DayCommand>().GetType());
	}
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
				b => b.AwaitCommands(DayCommandType.UpdateCfd).WithCondition("param", true, null))
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
				b => b.AwaitCommands(DayCommandType.UpdateCfd).WithCondition("param", true, null))
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
					.WithCondition("param1", true, null)
					.WithCondition("param2", true, null))
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
					.WithCondition("param1", true, null)
					.WithCondition("param2", true, null))
			.Build();

		var nextAwaited = scenario
			.GetNextAwaited(DayCommandType.UpdateTeamRoles, new { param1 = true, param2 = false });

		nextAwaited.toAdd.Should().BeEmpty();
	}
}