﻿namespace Domain.Game.Days.DayEvents.DayContainers;

public class UpdateCfdContainer
{
	public int DayId { get; }
	public int Released { get; }
	public int ReadyToDeploy { get; }
	public int WithTesters { get; }
	public int WithProgrammers { get; }
	public int WithAnalysts { get; }

	private UpdateCfdContainer(
		int dayId,
		int released,
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		DayId = dayId;
		Released = released;
		ReadyToDeploy = readyToDeploy;
		WithTesters = withTesters;
		WithProgrammers = withProgrammers;
		WithAnalysts = withAnalysts;
	}

	internal static UpdateCfdContainer CreateInstance(
		Day day,
		int released,
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		day.PostDayEvent(DayEventType.UpdateCfd);

		return new UpdateCfdContainer(
			day.Id,
			released,
			readyToDeploy,
			withTesters,
			withProgrammers,
			withAnalysts);
	}

	internal static UpdateCfdContainer None =>
		new(
			0,
			0,
			0,
			0,
			0,
			0);
}