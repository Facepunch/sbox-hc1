using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Skip to the next state if enough players are connected.
/// </summary>
public sealed class WaitForPlayers : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<UpdateStateEvent>
{
	[RequireComponent] public StateComponent State { get; private set; }

	[DeveloperCommand( "Pause Game Start", "Game Loop" )]
	public static void DevToggle()
	{
		GameMode.Instance
			?.Get<WaitForPlayers>()
			?.Toggle();
	}

	/// <summary>
	/// Only start the game if there are at least this many players.
	/// </summary>
	[Property, HostSync]
	public int MinPlayerCount { get; set; } = 2;

	/// <summary>
	/// Immediately start the game if this number of players are connected.
	/// </summary>
	[Property, HostSync]
	public int SkipPlayerCount { get; set; } = 10;

	[HostSync]
	public bool IsPostponed { get; set; }

	public float Remaining => State.DefaultDuration - Time.Now + GameMode.Instance.StateMachine.NextStateTime;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		IsPostponed = false;
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		var playerCount = GameUtils.PlayerPawns.Count();

		if ( IsPostponed || playerCount < MinPlayerCount )
		{
			GameMode.Instance.StateMachine.Transition( eventArgs.State.DefaultNextState!, eventArgs.State.DefaultDuration );
			return;
		}

		if ( playerCount >= SkipPlayerCount )
		{
			GameMode.Instance.StateMachine.Transition( eventArgs.State.DefaultNextState! );
		}
	}

	private void Toggle()
	{
		if ( IsPostponed ) Restart();
		else Postpone();
	}

	private void Postpone()
	{
		IsPostponed = true;

		GameMode.Instance.ShowStatusText( "Paused" );
		GameMode.Instance.HideTimer();
	}

	private void Restart()
	{
		State.Transition( State );
	}
}
