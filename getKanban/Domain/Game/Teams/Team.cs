using System.Text.RegularExpressions;
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
	public List<Participant> participants { get; } = null!;

	public Guid GameSessionId { get; }

	public Guid Id { get; }

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
		settings = TeamSessionSettings.Default();
	}

	public Team(Guid gameSessionId, string name)
		: this()
	{
		Id = Guid.NewGuid();
		GameSessionId = gameSessionId;
		this.name = name;
		participants = [];
		days = [];
	}

	public void AddPlayer(User user)
	{
		participants.Add(new Participant(user, ParticipantRole.Player));
	}

	public bool HasAccess(Guid userId)
	{
		var participant = participants.SingleOrDefault(p => p.User.Id == userId);
		if (participant is null)
		{
			return false;
		}

		return (participant.Role & ParticipantRole.Player) != 0
		       || (participant.Role & ParticipantRole.Angel) != 0;
	}

	public void StartSession()
	{
		currentDayNumber = 9;
		days.Add(ConfigureDay(currentDayNumber, []));
	}

	[GeneratedRegex(@"[\w-!.]+")]
	private static partial Regex NameRegexValidation();
}