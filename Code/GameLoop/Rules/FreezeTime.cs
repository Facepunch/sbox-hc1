using Facepunch.UI;
using Facepunch;
using System.Threading.Tasks;
using Sandbox.Events;

/// <summary>
/// Keep the players frozen for a few seconds at the start of each round.
/// </summary>
public sealed class FreezeTime : Component,
	IGameEventHandler<PreRoundStartEvent>,
	IGameEventHandler<PostRoundStartEvent>
{
	[Property, Sync]
	public float DurationSeconds { get; set; } = 15f;

	[Sync]
	public float StartTime { get; set; }

	void IGameEventHandler<PreRoundStartEvent>.OnGameEvent( PreRoundStartEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = true;
		}

		StartTime = Time.Now;

		GameMode.Instance.ShowCountDownTimer( StartTime, DurationSeconds );
		GameMode.Instance.ShowStatusText( "PREPARE" );

		GameMode.Instance.StartRound( DurationSeconds );
	}

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		GameMode.Instance.HideStatusText();

		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = false;
		}
	}
}
