using Facepunch;
using System.Threading.Tasks;
using Sandbox.Events;

/// <summary>
/// Wait for enough players to connect before starting, or start anyway if we waited too long.
/// </summary>
public sealed class WaitForPlayers : Component,
	IGameEventHandler<PreGameStartEvent>,
	IGameEventHandler<DuringGameStartEvent>
{
	[DeveloperCommand( "Pause Game Start", "Game Loop" )]
	public static void DevToggle()
	{
		GameMode.Instance
			?.Components.GetInDescendantsOrSelf<WaitForPlayers>()
			?.Toggle();
	}

	[Property, HostSync]
	public float DurationSeconds { get; set; } = 60f;

	[Property, HostSync]
	public float GameStartDelaySeconds { get; set; } = 5f;

	[Property, HostSync]
	public int MinPlayerCount { get; set; } = 2;

	[HostSync]
	public bool IsPostponed { get; set; }

	[HostSync]
	public float StartTime { get; set; }

	public float Remaining => DurationSeconds - Time.Now + StartTime;

	void IGameEventHandler<PreGameStartEvent>.OnGameEvent( PreGameStartEvent eventArgs )
	{
		Restart();
	}

	void IGameEventHandler<DuringGameStartEvent>.OnGameEvent( DuringGameStartEvent eventArgs )
	{
		if ( GameMode.Instance.NextState == GameState.RoundStart )
		{
			return;
		}

		if ( IsPostponed || GameUtils.AllPlayers.Count() < MinPlayerCount && Remaining > 0f )
		{
			return;
		}

		GameMode.Instance.ShowToast( "Match Starting..." );
		GameMode.Instance.ShowStatusText( "Starting in" );
		GameMode.Instance.ShowCountDownTimer( Time.Now, GameStartDelaySeconds );

		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = true;
		}

		GameMode.Instance.Transition( GameState.RoundStart, GameStartDelaySeconds );
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
		StartTime = Time.Now;
		IsPostponed = false;

		GameMode.Instance.ShowStatusText( "Waiting..." );
		GameMode.Instance.ShowCountDownTimer( StartTime, DurationSeconds );
	}
}
