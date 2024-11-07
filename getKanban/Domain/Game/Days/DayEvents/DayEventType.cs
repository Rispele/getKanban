namespace Domain.Game.Days.DayEvents;

public enum DayEventType
{
	Unknown = -1,
	WorkAnotherTeam = 0,
	UpdateTeamRoles = 1,
	RollDice = 2,
	ReleaseTickets = 3,
	EndOfReleaseTickets = 4,
	UpdateSprintBacklog = 5,
	EndOfUpdateSprintBacklog = 6,
	UpdateCfd = 7,
	EndOfUpdateCfd = 8,
	EndDay = 9
}