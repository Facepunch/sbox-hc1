
using Sandbox.Events;

/// <summary>
/// Keep track of how many rounds have been played;
/// </summary>
public sealed class RoundCounter : Component,
	IGameEventHandler<PreGameStartEvent>,
	IGameEventHandler<PreRoundStartEvent>
{
	/// <summary>
	/// Current round number, starting at 1.
	/// </summary>
	[Sync]
	public int Round { get; set; }

	void IGameEventHandler<PreGameStartEvent>.OnGameEvent( PreGameStartEvent eventArgs )
	{
		Round = 0;
	}

	[Early]
	void IGameEventHandler<PreRoundStartEvent>.OnGameEvent( PreRoundStartEvent eventArgs )
	{
		Round += 1;
	}
}
