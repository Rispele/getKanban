namespace Domain.Game.Days;

public enum DayCommandType
{
	Unknown = -1,
	WorkAnotherTeam = 0,
	UpdateTeamRoles = 1,
	RollDice = 2,
	ReleaseTickets = 3,
	UpdateSprintBacklog = 5,
	UpdateCfd = 7,
	EndDay = 9
}