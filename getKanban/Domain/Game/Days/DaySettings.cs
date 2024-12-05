﻿using Domain.Game.Days.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days;

[EntityTypeConfiguration(typeof(DaySettingsEntityTypeConfiguration))]
public class DaySettings
{
	public int Id { get; private init; }
	
	public int Number { get; init; }
	
	public int AnalystsCount { get; init; }
	public int ProgrammersCount { get; init; }
	public int TestersCount { get; init; }
	
	public bool CanReleaseNotImmediately { get; init; }
	
	public int ProfitPerClient { get; init; }
}