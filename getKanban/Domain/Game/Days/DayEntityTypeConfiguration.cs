﻿using System.Linq.Expressions;
using Domain.Game.Days.DayEvents;
using Domain.Game.Days.DayEvents.DayContainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Domain.Game.Days;

public class DayEntityTypeConfiguration : IEntityTypeConfiguration<Day>
{
	public void Configure(EntityTypeBuilder<Day> builder)
	{
		builder.HasKey(e => new { e.TeamSessionId, DayId = e.Id });

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

		ConfigureContainerRelation<WorkAnotherTeamContainer>(builder, d => d.WorkAnotherTeamContainer!, c => c.DayId);
		ConfigureContainerRelation<UpdateTeamRolesContainer>(builder, d => d.updateTeamRolesContainer!, d => d.DayId);
		ConfigureContainerRelation<RollDiceContainer>(builder, d => d.RollDiceContainer!, d => d.DayId);
		ConfigureContainerRelation<ReleaseTicketContainer>(builder, d => d.ReleaseTicketContainer!, d => d.DayId);
		ConfigureContainerRelation<UpdateSprintBacklogContainer>(builder, d => d.UpdateSprintBacklogContainer!, d => d.DayId);
		// ConfigureContainerRelation<UpdateCfdContainer>(builder, d => d.UpdateCfdContainer!, d => d.DayId);

		builder
			.HasOne(d => d.UpdateCfdContainer)
			.WithOne()
			.HasForeignKey<Day>();

		builder
			.HasMany<AwaitedEvent>()
			.WithOne();
	}

	private static void ConfigureContainerRelation<TContainer>(
		EntityTypeBuilder<Day> builder,
		Expression<Func<Day, TContainer>> expression,
		Expression<Func<TContainer, object?>> propertyExpression)
		where TContainer : class
	{
		builder
			.HasOne(expression)
			.WithOne()
			.HasPrincipalKey(propertyExpression);
	}

	private class ScenarioConverter : ValueConverter<Dictionary<DayEventType, List<DayEventType>>, string>
	{
		public ScenarioConverter()
			: base(
				context => JsonConvert.SerializeObject(context),
				str => JsonConvert.DeserializeObject<Dictionary<DayEventType, List<DayEventType>>>(str)!)
		{
		}
	}
}