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

	[Property, HostSync]
	public int MinPlayerCount { get; set; } = 2;

	[HostSync]
	public bool IsPostponed { get; set; }

	[HostSync]
	public float StartTime { get; set; }

	public float Remaining => State.DefaultDuration - Time.Now + StartTime;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		StartTime = Time.Now;
		IsPostponed = false;
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		if ( IsPostponed || GameUtils.AllPlayers.Count() < MinPlayerCount && Remaining > 0f )
		{
			return;
		}

		eventArgs.State.Transition();
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
