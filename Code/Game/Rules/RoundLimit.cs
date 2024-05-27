public sealed class RoundLimit : Component, IGameEndCondition
{
	[RequireComponent]
	public RoundCounter RoundCounter { get; private set; }

	[Property] public int MaxRounds { get; set; } = 30;

	public bool ShouldGameEnd()
	{
		return RoundCounter.Round >= MaxRounds;
	}
}
