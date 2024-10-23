namespace Domain.Game.Days.DayEvents;

public enum DayEventType
{
	Unknown = -1,
	WorkAnotherTeam = 0,
	UpdateTeamRoles = 1,
	RollDice = 2,
	ReleaseTasks = 3,
	UpdateSprintBacklog = 4,
	UpdateCfd = 5,
	EndDay = 6
}