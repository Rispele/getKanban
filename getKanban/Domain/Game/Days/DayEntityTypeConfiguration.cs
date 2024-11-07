﻿using System.Linq.Expressions;
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
		builder.HasKey(e => e.Id);

		builder.Property("scenario").HasConversion(new ScenarioConverter());

		builder.Ignore("currentlyAwaitedEvents");

		builder.Property(d => d.AnalystsNumber);
		builder.Property(d => d.ProgrammersNumber);
		builder.Property(d => d.TestersNumber);

		builder.Property(e => e.Number);

		builder.Property(e => e.Timestamp).ConfigureAsRowVersion();

		ConfigureContainerRelation<WorkAnotherTeamContainer>(builder, d => d.WorkAnotherTeamContainer!);
		ConfigureContainerRelation<UpdateTeamRolesContainer>(builder, d => d.UpdateTeamRolesContainer!);
		ConfigureContainerRelation<RollDiceContainer>(builder, d => d.RollDiceContainer!);
		ConfigureContainerRelation<ReleaseTicketContainer>(builder, d => d.ReleaseTicketContainer!);
		ConfigureContainerRelation<UpdateSprintBacklogContainer>(builder, d => d.UpdateSprintBacklogContainer!);
		ConfigureContainerRelation<UpdateCfdContainer>(builder, d => d.UpdateCfdContainer!);

		builder
			.HasMany<AwaitedEvent>("awaitedEvents")
			.WithOne()
			.OnDelete(DeleteBehavior.Cascade);

		builder.Navigation("awaitedEvents").AutoInclude();
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
		context => JsonConvert.SerializeObject(context),
		str => JsonConvert.DeserializeObject<Scenario>(str)!);
}