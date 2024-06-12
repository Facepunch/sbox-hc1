using Facepunch;
using System.Threading.Tasks;
using Sandbox.Diagnostics;
using Sandbox.Events;

/// <summary>
/// Handles the main game loop, using components that listen to state change
/// events to handle game logic.
/// </summary>
public sealed partial class GameMode : SingletonComponent<GameMode>, Component.INetworkListener
{
	/// <summary>
	/// Path in the scene of the game mode selected by the host.
	/// </summary>
	public static string ActivePath { get; set; }

	/// <summary>
	/// Current game state (controlled by the host.)
	/// </summary>
	[Property, HostSync, Obsolete]
	public GameState State { get; private set; }

	[HostSync, Obsolete]
	public GameState NextState { get; private set; } = GameState.GameStart;

	[HostSync, Obsolete]
	public float NextStateTime { get; private set; }

	[Property]
	public string Title { get; set; }

	[Property]
	public string Description { get; set; }

	protected override void OnAwake()
	{
		// Only stay enabled if host chose this game mode

		if ( ActivePath is { } path && !path.Equals( GameObject.GetScenePath(), StringComparison.OrdinalIgnoreCase ) )
		{
			GameObject.Enabled = false;
			return;
		}

		// Fallback for testing in editor - just use first active game mode

		if ( Instance is { IsValid: true, Active: true, Scene: {} scene } && scene == Scene )
		{
			Log.Info( $"A GameMode is already active, disabling {GameObject.GetScenePath()}" );
			GameObject.Enabled = false;
			return;
		}

		base.OnAwake();
	}

	void INetworkListener.OnBecameHost( Connection previousHost )
	{
		Log.Info( "We became the host, taking over the game loop..." );
	}

	[Obsolete]
	public void StartRound( float delaySeconds )
	{
		Assert.True( Networking.IsHost );
		Assert.True( State == GameState.RoundStart );

		Transition( GameState.DuringRound, delaySeconds );
	}

	[Obsolete]
	public void EndRound()
	{
		Assert.True( Networking.IsHost );
		Assert.True( State == GameState.DuringRound );

		Transition( GameState.RoundEnd );
	}

	[Obsolete]
	public void EndGame()
	{
		Assert.True( Networking.IsHost );
		Assert.True( State == GameState.RoundEnd );

		Transition( GameState.GameEnd, 3f );
	}

	/// <summary>
	/// Schedules a transition after the given delay, overriding any previously scheduled transition.
	/// </summary>
	[Obsolete]
	public void Transition( GameState nextState, float delaySeconds = 0f )
	{
		Assert.True( Networking.IsHost );

		NextState = nextState;
		NextStateTime = Time.Now + delaySeconds;
	}

	private void TransitionNow( GameState oldState, GameState newState )
	{
		Assert.True( Networking.IsHost );

		switch ( oldState )
		{
			case GameState.GameStart:
				Scene.Dispatch( new PostGameStartEvent() );
				break;

			case GameState.RoundStart:
				Scene.Dispatch( new PostRoundStartEvent() );
				break;

			case GameState.RoundEnd:
				Scene.Dispatch( new PostRoundEndEvent() );
				break;

			case GameState.GameEnd:
				Scene.Dispatch( new PostGameEndEvent() );
				break;
		}

		State = NextState;
		NextState = GameState.None;
		NextStateTime = float.PositiveInfinity;

		switch ( newState )
		{
			case GameState.GameStart:
				Scene.Dispatch( new PreGameStartEvent() );
				break;

			case GameState.RoundStart:
				Scene.Dispatch( new PreRoundStartEvent() );
				break;

			case GameState.RoundEnd:
				Scene.Dispatch( new PreRoundEndEvent() );
				break;

			case GameState.GameEnd:
				Scene.Dispatch( new PreGameEndEvent() );
				break;
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost ) return;

		if ( Time.Now > NextStateTime )
		{
			TransitionNow( State, NextState );
		}

		switch ( State )
		{
			case GameState.GameStart:
				Scene.Dispatch( new DuringGameStartEvent() );
				break;

			case GameState.RoundStart:
				Scene.Dispatch( new DuringRoundStartEvent() );
				break;

			case GameState.DuringRound:
				Scene.Dispatch( new DuringRoundEvent() );
				break;

			case GameState.RoundEnd:
				Scene.Dispatch( new DuringRoundEndEvent() );
				break;

			case GameState.GameEnd:
				Scene.Dispatch( new DuringGameEndEvent() );
				break;
		}
	}

	/// <summary>
	/// Gets the given component from within the game mode's object hierarchy, or null if not found / enabled.
	/// </summary>
	public T Get<T>()
		where T : Component
	{
		// TODO: cache, invalidate on state change

		return Components.GetInDescendantsOrSelf<T>();
	}

	/// <summary>
	/// RPC called by a client when they have finished respawning.
	/// </summary>
	[Authority]
	public void SendSpawnConfirmation( Guid playerGuid )
	{
		var player = Scene.Directory.FindComponentByGuid( playerGuid ) as PlayerController
			?? throw new Exception( $"Unknown {nameof(PlayerController)} Id: {playerGuid}" );

		Scene.Dispatch( new PlayerSpawnedEvent( player ) );
	}
}
