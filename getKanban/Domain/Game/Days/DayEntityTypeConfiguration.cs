using System.Linq.Expressions;
using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.RollDice;
using Domain.Game.Days.DayContainers.TeamMembers;
using Domain.Game.Days.Scenarios;
using Domain.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Domain.Game.Days;

public class DayEntityTypeConfiguration : IEntityTypeConfiguration<Day>
{
	public void Configure(EntityTypeBuilder<Day> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property("scenario").HasConversion(new ScenarioConverter());

		builder.Ignore("currentlyAwaitedEvents");

		builder.Property(e => e.Status);

		builder.Property(e => e.Timestamp).ConfigureAsRowVersion();

		ConfigureContainerRelation<WorkAnotherTeamContainer>(builder, d => d.WorkAnotherTeamContainer!);
		ConfigureContainerRelation<TeamMembersContainer>(builder, d => d.TeamMembersContainer!);
		ConfigureContainerRelation<RollDiceContainer>(builder, d => d.DiceRollContainer!);
		ConfigureContainerRelation<ReleaseTicketContainer>(builder, d => d.ReleaseTicketContainer!);
		ConfigureContainerRelation<UpdateSprintBacklogContainer>(builder, d => d.UpdateSprintBacklogContainer!);
		ConfigureContainerRelation<UpdateCfdContainer>(builder, d => d.UpdateCfdContainer!);

		builder
			.HasOne<DaySettings>(t => t.DaySettings)
			.WithOne()
			.OnDelete(DeleteBehavior.Cascade);
		
		builder
			.HasMany<AwaitedCommands>("awaitedCommands")
			.WithOne()
			.OnDelete(DeleteBehavior.Cascade);

		builder.Navigation("awaitedCommands").AutoInclude();
		builder.Navigation(t => t.DaySettings).AutoInclude();
	}

	private static void ConfigureContainerRelation<TContainer>(
		EntityTypeBuilder<Day> builder,
		Expression<Func<Day, TContainer?>> expression)
		where TContainer : class
	{
		builder
			.HasOne(expression)
			.WithOne()
			.HasForeignKey<TContainer>("day_id")
			.OnDelete(DeleteBehavior.Cascade);

		builder.Navigation(expression).AutoInclude();
	}

	private class ScenarioConverter() : ValueConverter<Scenario, string>(
		context => context.ToJson(),
		str => JsonConvert.DeserializeObject<Scenario>(str)!);
}