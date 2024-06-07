/// <summary>
/// End the game after a fixed number of rounds.
/// </summary>
public sealed class RoundLimit : Component, IGameEndCondition
{
	[RequireComponent]
	public RoundCounter RoundCounter { get; private set; }

	[Property, Sync] public int MaxRounds { get; set; } = 30;

	public bool ShouldGameEnd()
	{
		return RoundCounter.Round >= MaxRounds;
	}
}
