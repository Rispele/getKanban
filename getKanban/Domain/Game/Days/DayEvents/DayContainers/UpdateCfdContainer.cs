﻿using Domain.DomainExceptions;
using Domain.Game.Days.DayEvents.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(UpdateCfdContainerEntityTypeConfiguration))]
public class UpdateCfdContainer
{
	public long Id { get; }
	public int? Released { get; private set; }
	public int? ToDeploy { get; private set; }
	public int? WithTesters { get; private set; }
	public int? WithProgrammers { get; private set; }
	public int? WithAnalysts { get; private set; }

	public UpdateCfdContainer()
	{
	}

	private UpdateCfdContainer(
		int released,
		int toDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		Released = released;
		ToDeploy = toDeploy;
		WithTesters = withTesters;
		WithProgrammers = withProgrammers;
		WithAnalysts = withAnalysts;
	}

	internal void Update(UpdateCfdContainerPatchType patchType, int value)
	{
		if (value < 0)
		{
			throw new DomainException("Value cannot be negative.");
		}

		switch (patchType)
		{
			case UpdateCfdContainerPatchType.Released:
				Released = value;
				break;
			case UpdateCfdContainerPatchType.ToDeploy:
				ToDeploy = value;
				break;
			case UpdateCfdContainerPatchType.WithTesters:
				WithTesters = value;
				break;
			case UpdateCfdContainerPatchType.WithProgrammers:
				WithProgrammers = value;
				break;
			case UpdateCfdContainerPatchType.WithAnalysts:
				WithAnalysts = value;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(patchType), patchType, null);
		}
	}

	internal static UpdateCfdContainer None =>
		new(
			0,
			0,
			0,
			0,
			0);
}