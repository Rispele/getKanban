using WebApp.Models;

namespace Core.Dtos;

public class CfdGraphDto
{
	public readonly Dictionary<string, List<(int, int)>> GraphPointsPerLabel = new ()
	{
		{ "Работа аналитиков", [] },
		{ "Работа разработчиков", [] },
		{ "Работа тестировщиков", [] },
		{ "Готовы к поставке", [] },
		{ "Поставлено", [] }
	};
}