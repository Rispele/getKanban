using Domain.Game.Days;
using Domain.Game.Days.DayEvents;

namespace Domain.Game.Teams;

public class TeamSession
{
	private readonly List<Day> days;
	private readonly TeamSessionSettings sessionSettings;

	private int currentDayNumber;

	private Day CurrentDay => days[currentDayNumber - 9];

	public TeamSession()
	{
		sessionSettings = new TeamSessionSettings();
		currentDayNumber = 9;
		days = new List<Day>();
	}

	//TODO: пока что так, потом мб декларативно опишем
	private DayContext ConfigureDayContext(
		bool anotherTeamAppeared,
		bool shouldRelease,
		bool shouldUpdateSprintBacklog)
	{
		var scenario = new Dictionary<DayEventType, List<DayEventType>>();

		if (anotherTeamAppeared)
		{
			scenario[DayEventType.WorkAnotherTeam] =
			[
				DayEventType.UpdateTeamRoles,
				DayEventType.RollDice
			];
		}

		scenario[DayEventType.UpdateTeamRoles] = [DayEventType.UpdateTeamRoles];
		if (shouldRelease && shouldUpdateSprintBacklog)
		{
			scenario[DayEventType.RollDice] = [DayEventType.ReleaseTickets];
			scenario[DayEventType.ReleaseTickets] = [DayEventType.UpdateSprintBacklog];
			scenario[DayEventType.UpdateSprintBacklog] = [DayEventType.UpdateCfd];
		}
		else if (shouldRelease)
		{
			scenario[DayEventType.RollDice] = [DayEventType.ReleaseTickets];
			scenario[DayEventType.ReleaseTickets] = [DayEventType.UpdateCfd];
		}
		else if (shouldUpdateSprintBacklog)
		{
			scenario[DayEventType.RollDice] = [DayEventType.UpdateSprintBacklog];
			scenario[DayEventType.UpdateSprintBacklog] = [DayEventType.UpdateCfd];
		}
		else
		{
			scenario[DayEventType.RollDice] = [DayEventType.UpdateCfd];
		}

		scenario[DayEventType.UpdateCfd] = [DayEventType.EndDay];

		return new DayContext(
			++currentDayNumber,
			scenario,
			anotherTeamAppeared ? [DayEventType.WorkAnotherTeam] : [DayEventType.UpdateTeamRoles, DayEventType.RollDice]
		);
	}
}