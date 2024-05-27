using Facepunch;
using System.Threading.Tasks;

/// <summary>
/// Handles the main game loop, using components that listen to state change
/// events to handle game logic.
/// </summary>
public sealed class GameMode : SingletonComponent<GameMode>
{
	/// <summary>
	/// Current game state.
	/// </summary>
	[Property, Sync]
	public GameState State { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy )
		{
			return;
		}

		_ = RunGame();
	}

	/// <summary>
	/// Main game loop.
	/// </summary>
	public async Task RunGame()
	{
		State = GameState.PreGame;

		await StartGame();

		while ( !ShouldGameEnd() )
		{
			State = GameState.PreRound;

			await StartRound();

			State = GameState.DuringRound;

			while ( !ShouldRoundEnd() )
			{
				await Task.FixedUpdate();
			}

			State = GameState.PostRound;

			await EndRound();
		}

		State = GameState.PostGame;

		await EndGame();
	}

	private async Task StartGame()
	{
		await Dispatch<IGameStartListener>(
			x => x.PreGameStart(),
			x => x.OnGameStart(),
			x => x.PostGameStart() );
	}

	private async Task StartRound()
	{
		await Dispatch<IRoundStartListener>(
			x => x.PreRoundStart(),
			x => x.OnRoundStart(),
			x => x.PostRoundStart() );
	}

	private async Task EndRound()
	{
		await Dispatch<IRoundEndListener>(
			x => x.PreRoundEnd(),
			x => x.OnRoundEnd(),
			x => x.PostRoundEnd() );
	}

	private async Task EndGame()
	{
		await Dispatch<IGameEndListener>(
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

	public Transform GetSpawnTransform( Team team )
	{
		return GetSingleOrThrow<ISpawnPointAssigner>()
			.GetSpawnTransform( team );
	}

	private async Task Dispatch<T>( Action<T> pre, Func<T, Task> on, Action<T> post )
	{
		var components = Components.GetAll<T>().ToArray();

		foreach (var comp in components)
		{
			pre( comp );
		}

		await Task.WhenAll( Components.GetAll<T>().Select( on ) );

		foreach (var comp in components)
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
