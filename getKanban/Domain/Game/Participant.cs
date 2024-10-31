﻿using Domain.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game;

[PrimaryKey(nameof(Id))]
public class Participant
{
	public long Id { get; }
	public ParticipantRole Role { get; }
	public User User { get; } = null!;

	[UsedImplicitly]
	private Participant()
	{
	}

	public Participant(User user, ParticipantRole role)
	{
		User = user;
		Role = role;
	}
}