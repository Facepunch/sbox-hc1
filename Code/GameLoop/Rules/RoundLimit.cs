using Sandbox.Events;/// <summary>
/// End the game after a fixed number of rounds.
/// </summary>
public sealed class RoundLimit : Component,
	IGameEventHandler<PreRoundEndEvent>
{
	[RequireComponent]
	public RoundCounter RoundCounter { get; private set; }

	[Property, Sync] public int MaxRounds { get; set; } = 30;

	void IGameEventHandler<PreRoundEndEvent>.OnGameEvent( PreRoundEndEvent eventArgs )
	{
		if ( RoundCounter.Round >= MaxRounds )
		{
			GameMode.Instance.EndGame();
		}
	}
}
