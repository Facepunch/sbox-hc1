using System.Threading.Tasks;
using Facepunch;
using Facepunch.UI;
using Sandbox.Events;

/// <summary>
/// Wait a bit before starting the next round.
/// </summary>
public class NextRoundDelay : Component,
	IGameEventHandler<PreRoundEndEvent>
{
	[Property, Sync]
	public float DurationSeconds { get; set; } = 5f;

	[Early]
	void IGameEventHandler<PreRoundEndEvent>.OnGameEvent( PreRoundEndEvent eventArgs )
	{
		GameMode.Instance.Transition( GameState.RoundStart, DurationSeconds );
	}
}
