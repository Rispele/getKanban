namespace Domain.Game.Tickets;

public static class TicketDescriptors
{
	public const int DefaultClientOffRate = 2;
	public const int ZeroClientOffRate = 0;

	public static readonly TicketDescriptor S01 = new(
		nameof(S01),
		21,
		DefaultClientOffRate,
		Number: 1);

	public static readonly TicketDescriptor S02 = new(
		nameof(S02),
		22,
		DefaultClientOffRate,
		Number: 2);

	public static readonly TicketDescriptor S03 = new(
		nameof(S03),
		24,
		DefaultClientOffRate,
		Number: 3);

	public static readonly TicketDescriptor S04 = new(
		nameof(S04),
		26,
		DefaultClientOffRate,
		Number: 4);

	public static readonly TicketDescriptor S05 = new(
		nameof(S05),
		25,
		DefaultClientOffRate,
		Number: 5);

	public static readonly TicketDescriptor S06 = new(
		nameof(S06),
		28,
		DefaultClientOffRate,
		Number: 6);

	public static readonly TicketDescriptor S07 = new(
		nameof(S07),
		27,
		DefaultClientOffRate,
		Number: 7);

	public static readonly TicketDescriptor S08 = new(
		nameof(S08),
		25,
		DefaultClientOffRate,
		Number: 8);

	public static readonly TicketDescriptor S09 = new(
		nameof(S09),
		28,
		DefaultClientOffRate,
		Number: 9);

	public static readonly TicketDescriptor S10 = new(
		nameof(S10),
		26,
		DefaultClientOffRate,
		Number: 10);

	public static readonly TicketDescriptor S11 = new(
		nameof(S11),
		22,
		DefaultClientOffRate,
		Number: 11);

	public static readonly TicketDescriptor S12 = new(
		nameof(S12),
		26,
		DefaultClientOffRate,
		Number: 12);

	public static readonly TicketDescriptor S13 = new(
		nameof(S13),
		23,
		DefaultClientOffRate,
		Number: 13);

	public static readonly TicketDescriptor S14 = new(
		nameof(S14),
		25,
		DefaultClientOffRate,
		Number: 14);

	public static readonly TicketDescriptor S15 = new(
		nameof(S15),
		19,
		DefaultClientOffRate,
		Number: 15);

	public static readonly TicketDescriptor S16 = new(
		nameof(S16),
		22,
		DefaultClientOffRate,
		Number: 16);

	public static readonly TicketDescriptor S17 = new(
		nameof(S17),
		20,
		DefaultClientOffRate,
		Number: 17);

	public static readonly TicketDescriptor S18 = new(
		nameof(S18),
		22,
		DefaultClientOffRate,
		Number: 18);

	public static readonly TicketDescriptor S19 = new(
		nameof(S19),
		22,
		DefaultClientOffRate,
		Number: 19);

	public static readonly TicketDescriptor S20 = new(
		nameof(S20),
		21,
		DefaultClientOffRate,
		Number: 20);

	public static readonly TicketDescriptor S21 = new(
		nameof(S21),
		28,
		DefaultClientOffRate,
		Number: 21);

	public static readonly TicketDescriptor S22 = new(
		nameof(S22),
		22,
		DefaultClientOffRate,
		Number: 22);

	public static readonly TicketDescriptor S23 = new(
		nameof(S23),
		23,
		DefaultClientOffRate,
		Number: 23);

	public static readonly TicketDescriptor S24 = new(
		nameof(S24),
		25,
		DefaultClientOffRate,
		Number: 24);

	public static readonly TicketDescriptor S25 = new(
		nameof(S25),
		22,
		DefaultClientOffRate,
		Number: 25);

	public static readonly TicketDescriptor S26 = new(
		nameof(S26),
		32,
		DefaultClientOffRate,
		Number: 26);

	public static readonly TicketDescriptor S27 = new(
		nameof(S27),
		20,
		DefaultClientOffRate,
		Number: 27);

	public static readonly TicketDescriptor S28 = new(
		nameof(S28),
		24,
		DefaultClientOffRate,
		Number: 28);

	public static readonly TicketDescriptor S29 = new(
		nameof(S29),
		23,
		DefaultClientOffRate,
		Number: 29);

	public static readonly TicketDescriptor S30 = new(
		nameof(S30),
		32,
		DefaultClientOffRate,
		Number: 30);

	public static readonly TicketDescriptor S31 = new(
		nameof(S31),
		24,
		DefaultClientOffRate,
		Number: 31);

	public static readonly TicketDescriptor S32 = new(
		nameof(S32),
		25,
		DefaultClientOffRate,
		Number: 32);

	public static readonly TicketDescriptor S33 = new(
		nameof(S33),
		24,
		DefaultClientOffRate,
		Number: 33);

	public static readonly TicketDescriptor S34 = new(
		nameof(S34),
		22,
		DefaultClientOffRate,
		Number: 34);

	public static readonly TicketDescriptor S35 = new(
		nameof(S35),
		25,
		DefaultClientOffRate,
		Number: 35);

	public static readonly TicketDescriptor S36 = new(
		nameof(S36),
		24,
		DefaultClientOffRate,
		Number: 36);

	public static readonly TicketDescriptor AutoRelease = new(
		"I01",
		0,
		ZeroClientOffRate,
		false,
		CanBeReleasedImmediately: true,
		OnRelease: team => team.CurrentDay.ReleaseTicketContainer.CanReleaseNotImmediately(),
		OnRemove: team => team.CurrentDay.ReleaseTicketContainer.CanReleaseNotImmediately(false));

	public static readonly TicketDescriptor AutoTests = new(
		"I02",
		0,
		ZeroClientOffRate,
		false,
		CanBeReleasedImmediately: true);

	public static readonly TicketDescriptor LawTask = new(
		"F01",
		0,
		ZeroClientOffRate,
		false,
		Penalty: new TicketPenaltyDescriptor(2500, 15));

	public static readonly TicketDescriptor BusinessTask = new(
		"E01",
		0,
		ZeroClientOffRate,
		false,
		Bonus: new TicketBonusDescriptor(4000, 18));

	private static IReadOnlyList<TicketDescriptor> BusinessTicketDescriptors { get; } =
	[
		S01, S02, S03, S04, S05, S06, S07, S08, S09, S10,
		S11, S12, S13, S14, S15, S16, S17, S18, S19, S20,
		S21, S22, S23, S24, S25, S26, S27, S28, S29, S30,
		S31, S32, S33, S34, S35, S36
	];

	private static IReadOnlyList<TicketDescriptor> InfrastructureTicketDescriptors { get; } = [AutoRelease, AutoTests];

	public static IReadOnlyList<TicketDescriptor> AllTicketDescriptors { get; } =
		BusinessTicketDescriptors
			.Concat(InfrastructureTicketDescriptors)
			.Concat([LawTask, BusinessTask])
			.ToArray();

	public static IReadOnlyDictionary<string, TicketDescriptor> TicketDescriptorsById { get; } =
		AllTicketDescriptors.ToDictionary(t => t.Id, t => t);

	public static TicketDescriptor GetByTicketId(string ticketId)
	{
		return TicketDescriptorsById[ticketId];
	}
}