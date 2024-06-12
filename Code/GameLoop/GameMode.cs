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
	[Property, HostSync]
	public GameState State { get; private set; }

	[HostSync]
	public GameState NextState { get; private set; } = GameState.GameStart;

	[HostSync]
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

	public void StartRound( float delaySeconds )
	{
		Assert.True( Networking.IsHost );
		Assert.True( State == GameState.RoundStart );

		Transition( GameState.DuringRound, delaySeconds );
	}

	public void EndRound()
	{
		Assert.True( Networking.IsHost );
		Assert.True( State == GameState.DuringRound );

		Transition( GameState.RoundEnd );
	}

	public void EndGame()
	{
		Assert.True( Networking.IsHost );
		Assert.True( State == GameState.RoundEnd );

		Transition( GameState.GameEnd, 3f );
	}

	/// <summary>
	/// Schedules a transition after the given delay, overriding any previously scheduled transition.
	/// </summary>
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
	/// RPC called by a client when they have finished respawning.
	/// </summary>
	[Authority]
	public void SendSpawnConfirmation( Guid playerGuid )
	{
		var player = Scene.Directory.FindComponentByGuid( playerGuid ) as PlayerController;
		if ( player.IsValid() )
		{
			_ = SpawnPlayer( player );
		}
	}

	public Task SpawnPlayer( PlayerController player )
	{
		Assert.True( Networking.IsHost );
		
		return Dispatch<IPlayerSpawnListener>(
			x => x.PrePlayerSpawn( player ),
			x => x.OnPlayerSpawn( player ),
			x => x.PostPlayerSpawn( player ) );
	}

	private async Task Dispatch<T>( Action<T> pre, Func<T, Task> on, Action<T> post )
	{
		var components = Scene.GetAllComponents<T>().ToArray();

		foreach ( var comp in components )
		{
			pre( comp );
		}

		await Task.WhenAll( components.Select( on ) );

		foreach ( var comp in components )
		{
			post( comp );
		}
	}

	private T GetSingleOrThrow<T>()
	{
		return Components.GetInDescendantsOrSelf<T>()
			?? throw new Exception( $"Missing required component {typeof( T ).Name}." );
	}
}
