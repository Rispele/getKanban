using Domain.Game.Teams.Configurations;
using Domain.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Teams;

[EntityTypeConfiguration(typeof(TeamEntityTypeConfiguration))]
public partial class Team
{
	private readonly List<Participant> participants = null!;

	public Guid GameSessionId { get; }

	public Guid Id { get; }

	public string Name { get; private set; } = null!;

	[UsedImplicitly]
	private Team()
	{
		settings = TeamSessionSettings.Default();
		TakenTickets = new Lazy<HashSet<string>>(BuildTakenTickets);
		TicketsInWork = new Lazy<HashSet<string>>(BuildTicketsInWork);
		ReleasedTickets = new Lazy<HashSet<string>>(BuildReleasedTickets);
		AnotherTeamScores = new Lazy<int>(BuildAnotherTeamScores);
	}

	public Team(Guid gameSessionId, string name)
	 : this()
	{
		Id = Guid.NewGuid();
		GameSessionId = gameSessionId;
		Name = name;
		participants = [];
		currentDayNumber = 9;
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
}