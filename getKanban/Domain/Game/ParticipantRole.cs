namespace Domain.Game;

[Flags]
public enum ParticipantRole
{
	Creator = 1,
	Angel = 1 << 1,
	Player = 2 << 1
}