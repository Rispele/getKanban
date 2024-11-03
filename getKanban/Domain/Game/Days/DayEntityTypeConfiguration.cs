using System.Linq.Expressions;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;
using Domain.Game.Days.Scenarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Domain.Game.Days;

public class DayEntityTypeConfiguration : IEntityTypeConfiguration<Day>
{
	public void Configure(EntityTypeBuilder<Day> builder)
	{
		builder.HasKey(e => new { TeamSessionId = e.TeamId, DayId = e.Id });

		builder.Property(e => e.Id)
			.ValueGeneratedOnAdd()
			.IsRequired();

		builder.Property("scenario")
			.HasConversion(new ScenarioConverter());

		builder.Ignore("currentlyAwaitedEvents");

		builder.Property("analystsNumber");
		builder.Property("programmersNumber");
		builder.Property("testersNumber");

		builder.Property(e => e.Timestamp).IsRowVersion();

		ConfigureContainerRelation<WorkAnotherTeamContainer>(builder, d => d.WorkAnotherTeamContainer!);
		ConfigureContainerRelation<UpdateTeamRolesContainer>(builder, d => d.UpdateTeamRolesContainer!);
		ConfigureContainerRelation<RollDiceContainer>(builder, d => d.RollDiceContainer!);
		ConfigureContainerRelation<ReleaseTicketContainer>(builder, d => d.ReleaseTicketContainer!);
		ConfigureContainerRelation<UpdateSprintBacklogContainer>(builder, d => d.UpdateSprintBacklogContainer!);
		ConfigureContainerRelation<UpdateCfdContainer>(builder, d => d.UpdateCfdContainer!);

		builder
			.HasMany<AwaitedEvent>()
			.WithOne();
	}

	private static void ConfigureContainerRelation<TContainer>(
		EntityTypeBuilder<Day> builder,
		Expression<Func<Day, TContainer>> expression)
		where TContainer : class
	{
		builder
			.HasOne(expression)
			.WithOne()
			.HasForeignKey<Day>();
	}

	private class ScenarioConverter() : ValueConverter<Scenario, string>(
		context => JsonConvert.SerializeObject(context),
		str => JsonConvert.DeserializeObject<Scenario>(str)!);
}