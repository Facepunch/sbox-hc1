using Facepunch;
using System.Threading.Tasks;
using Sandbox.Diagnostics;

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

	[Property]
	public string Title { get; set; }

	[Property]
	public string Description { get; set; }

	private Task _gameLoopTask;

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

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy )
			return;

		_ = ResumeGame();
	}

	void INetworkListener.OnBecameHost( Connection previousHost )
	{
		Log.Info( "We became the host, taking over the game loop..." );
		_ = ResumeGame();
	}

	private Task ResumeGame()
	{
		return _gameLoopTask ??= GameLoop();
	}
	
	private async Task GameLoop()
	{
		while ( State != GameState.Ended )
		{
			switch ( State )
			{
				case GameState.PreGame:
					await StartGame();

					State = GameState.PreRound;
					break;

				case GameState.PreRound:
					await StartRound();

					State = GameState.DuringRound;
					break;

				case GameState.DuringRound:
					await Task.FixedUpdate();

					State = ShouldRoundEnd()
						? GameState.PostRound
						: GameState.DuringRound;
					break;

				case GameState.PostRound:
					await EndRound();

					State = ShouldGameEnd()
						? GameState.PostGame
						: GameState.PreRound;
					break;

				case GameState.PostGame:
					await EndGame();

					State = GameState.Ended;
					break;
			}
		}
	}

	private Task StartGame()
	{
		ShowStatusText( "Preparing..." );
		HideTimer();

		return Dispatch<IGameStartListener>(
			x => x.PreGameStart(),
			x => x.OnGameStart(),
			x => x.PostGameStart() );
	}

	private Task StartRound()
	{
		ShowStatusText( "Starting Round..." );
		HideTimer();

		return Dispatch<IRoundStartListener>(
			x => x.PreRoundStart(),
			x => x.OnRoundStart(),
			x => x.PostRoundStart() );
	}

	private Task EndRound()
	{
		return Dispatch<IRoundEndListener>(
			x => x.PreRoundEnd(),
			x => x.OnRoundEnd(),
			x => x.PostRoundEnd() );
	}

	private Task EndGame()
	{
		ShowStatusText( "Ending Game..." );
		HideTimer();

		return Dispatch<IGameEndListener>(
			x => x.PreGameEnd(),
			x => x.OnGameEnd(),
			x => x.PostGameEnd() );
	}

	private bool ShouldGameEnd()
	{
		return Components.GetAll<IGameEndCondition>()
			.Any( x => x.ShouldGameEnd() );
	}

	private bool ShouldRoundEnd()
	{
		return Components.GetAll<IRoundEndCondition>()
			.Any( x => x.ShouldRoundEnd() );
	}

	/// <summary>
	/// RPC called by a client when they have finished respawning.
	/// </summary>
	[Authority]
	public void SendSpawnConfirmation()
	{
		var player = GameUtils.AllPlayers.FirstOrDefault( x => x.Network.OwnerConnection == Rpc.Caller )
			?? throw new Exception( $"Unable to find {nameof(PlayerController)} owned by {Rpc.Caller.DisplayName}." );

		_ = SpawnPlayer( player );
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
