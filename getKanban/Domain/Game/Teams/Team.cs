using Domain.Game.Days.DayEvents.DayContainers;
using Domain.Users;

namespace Domain.Game.Teams;

public class Team
{
	private readonly List<Participant> participants;

	private readonly TeamSession teamSession;

	public Team(Guid gameSessionId, string name)
	{
		Id = Guid.NewGuid();
		GameSessionId = gameSessionId;
		Name = name;
		teamSession = new TeamSession(Id);
		participants = [];
	}

	public Guid GameSessionId { get; }

	public Guid Id { get; }

	public string Name { get; private set; }

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

	public int RollDiceForAnotherTeam()
	{
		return teamSession.RollDiceForAnotherTeam();
	}

	public void UpdateTeamRoles(TeamRole from, TeamRole to)
	{
		teamSession.UpdateTeamRoles(from, to);
	}

	public void RollDices()
	{
		teamSession.RollDices();
	}

	public void ReleaseTickets(string[] ticketIds)
	{
		teamSession.ReleaseTickets(ticketIds);
	}

	public void UpdateSprintBacklog(string[] ticketIds)
	{
		teamSession.UpdateSprintBacklog(ticketIds);
	}

	public void UpdateCfd(
		int readyToDeploy,
		int withTesters,
		int withProgrammers,
		int withAnalysts)
	{
		teamSession.UpdateCfd(readyToDeploy, withTesters, withProgrammers, withAnalysts);
	}

	public void EndDay()
	{
		teamSession.EndDay();
	}
}