using Facepunch;
using System.Threading.Tasks;

public sealed class GameMode : SingletonComponent<GameMode>
{
	/// <summary>
	/// Current game state.
	/// </summary>
	[Property, Sync]
	public GameState State { get; private set; }

	private async Task Dispatch<T>( Action<T> pre, Func<T, Task> on, Action<T> post )
	{
		var components = Components.GetAll<T>().ToArray();

		foreach ( var comp in components )
		{
			pre( comp );
		}

		await Task.WhenAll( Components.GetAll<T>().Select( on ) );

		foreach ( var comp in components )
		{
			post( comp );
		}
	}

	private T GetSingleOrThrow<T>()
	{
		return Components.GetInDescendantsOrSelf<T>()
			?? throw new Exception( $"Missing required component {typeof(T).Name}." );
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy )
		{
			return;
		}

		_ = StartGame();
	}

	public async Task StartGame()
	{
		Log.Info( $"{GameObject.Name}: {nameof(StartGame)}" );

		State = GameState.PreGame;

		await Dispatch<IGameStartListener>(
			x => x.PreGameStart(),
			x => x.OnGameStart(),
			x => x.PostGameStart() );

		await StartRound();
	}

	public async Task StartRound()
	{
		Log.Info( $"{GameObject.Name}: {nameof( StartRound )}" );

		State = GameState.PreRound;

		await Dispatch<IRoundStartListener>(
			x => x.PreRoundStart(),
			x => x.OnRoundStart(),
			x => x.PostRoundStart() );

		State = GameState.DuringRound;
	}

	protected override void OnUpdate()
	{
		if ( State == GameState.DuringRound )
		{
			if ( ShouldRoundEnd() )
			{
				_ = EndRound();
			}
		}
	}

	public async Task EndRound()
	{
		Log.Info( $"{GameObject.Name}: {nameof( EndRound )}" );

		State = GameState.PostRound;

		await Dispatch<IRoundEndListener>(
			x => x.PreRoundEnd(),
			x => x.OnRoundEnd(),
			x => x.PostRoundEnd() );

		if ( ShouldGameEnd() )
		{
			await EndGame();
		}
		else
		{
			await StartRound();
		}
	}

	public async Task EndGame()
	{
		Log.Info( $"{GameObject.Name}: {nameof( EndGame )}" );

		State = GameState.PostGame;

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
}
