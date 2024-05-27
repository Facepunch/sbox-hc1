using Facepunch;
using System.Threading.Tasks;

public sealed class GameMode : SingletonComponent<GameMode>
{
	/// <summary>
	/// Current game state.
	/// </summary>
	public GameState State { get; set; }

	private Task Dispatch<T>( Func<T, Task> handler )
	{
		return Task.WhenAll( Components.GetAll<T>().Select( handler ) );
	}

	private T GetSingleOrThrow<T>()
	{
		return Components.GetInDescendantsOrSelf<T>()
			?? throw new Exception( $"Missing required component {typeof(T).Name}." );
	}

	public async Task StartGame()
	{
		State = GameState.PreGame;

		await Dispatch<IGameStartListener>( x => x.OnGameStart() );

		await StartRound();
	}

	public async Task StartRound()
	{
		State = GameState.PreRound;

		await Dispatch<IRoundStartListener>( x => x.OnRoundStart() );

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
		State = GameState.PostRound;

		await Dispatch<IRoundEndListener>( x => x.OnRoundEnd() );

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
		State = GameState.PostGame;

		await Dispatch<IGameEndListener>( x => x.OnGameEnd() );
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
