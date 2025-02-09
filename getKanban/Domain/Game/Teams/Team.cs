﻿using System.Text.RegularExpressions;
using Domain.DomainExceptions;
using Domain.Game.Teams.Configurations;
using Domain.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Teams;

[EntityTypeConfiguration(typeof(TeamEntityTypeConfiguration))]
public partial class Team
{
	private string name = null!;
	public Guid GameSessionId { get; }
	public Guid Id { get; }

	public ParticipantsContainer Players { get; } = null!;

	public long RowVersions { get; [UsedImplicitly] private set; }

	public string Name
	{
		get => name;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new DomainException("Name cannot be empty");
			}

			if (value.Length > 100)
			{
				throw new DomainException("Name cannot be longer than 100 characters");
			}

			var regex = NameRegexValidation();
			if (!regex.IsMatch(value))
			{
				throw new DomainException("Invalid name");
			}

			name = value;
		}
	}

	[UsedImplicitly]
	private Team()
	{
		Settings = TeamSessionSettings.Default();
	}

	public Team(Guid gameSessionId, string name)
		: this()
	{
		Id = Guid.NewGuid();
		GameSessionId = gameSessionId;
		Settings = TeamSessionSettings.Default();

		Name = name;
		Players = new ParticipantsContainer(gameSessionId, Id);
		days = [];
	}

	public (bool matched, bool updated) AddByInviteCode(User user, string inviteCode)
	{
		return Players.AddParticipantIfMatchInviteCode(inviteCode, user, ParticipantRole.Player);
	}

	public void EnsureHasAccess(Guid userId)
	{
		if (!HasAccess(userId))
		{
			throw new DomainException("User does not have access to this team");
		}
	}

	public bool HasAccess(Guid userId)
	{
		var participant = Players.Participants.SingleOrDefault(p => p.User.Id == userId);
		if (participant is null)
		{
			return false;
		}

		return (participant.Role & ParticipantRole.Player) != 0;
	}

	public void StartSession()
	{
		currentDayNumber = 9;
		days.Add(ConfigureDay(currentDayNumber, []));
	}

	[GeneratedRegex(@"[\w-!.]+")]
	private static partial Regex NameRegexValidation();
}