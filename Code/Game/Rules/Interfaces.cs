using System.Threading.Tasks;
using Facepunch;

public interface IGameEndCondition
{
	/// <summary>
	/// Called on the host at the end of a round to determine if the game should end.
	/// </summary>
	bool ShouldGameEnd();
}

public interface IRoundEndCondition
{
	/// <summary>
	/// Called on the host every update during a round to determine if it should end.
	/// </summary>
	bool ShouldRoundEnd();
}

public interface IGameStartListener
{
	/// <summary>
	/// Called on the host before <see cref="OnGameStart"/>.
	/// </summary>
	public void PreGameStart() { }

	/// <summary>
	/// Called on the host when a game is starting.
	/// </summary>
	public Task OnGameStart() => Task.CompletedTask;

	/// <summary>
	/// Called on the host after <see cref="OnGameStart"/> completes.
	/// </summary>
	public void PostGameStart() { }
}

public interface IGameEndListener
{
	/// <summary>
	/// Called on the host before <see cref="OnGameEnd"/>.
	/// </summary>
	public void PreGameEnd() { }

	/// <summary>
	/// Called on the host when a game is ending.
	/// </summary>
	public Task OnGameEnd() => Task.CompletedTask;

	/// <summary>
	/// Called on the host after <see cref="OnGameEnd"/> completes.
	/// </summary>
	public void PostGameEnd() { }
}

public interface IRoundStartListener
{
	/// <summary>
	/// Called on the host before <see cref="OnRoundStart"/>.
	/// </summary>
	public void PreRoundStart() { }

	/// <summary>
	/// Called on the host when a round is starting.
	/// </summary>
	public Task OnRoundStart() => Task.CompletedTask;

	/// <summary>
	/// Called on the host after <see cref="OnRoundStart"/> completes.
	/// </summary>
	public void PostRoundStart() { }
}

public interface IRoundEndListener
{
	/// <summary>
	/// Called on the host before <see cref="OnRoundEnd"/>.
	/// </summary>
	public void PreRoundEnd() { }

	/// <summary>
	/// Called on the host when a round is ending.
	/// </summary>
	public Task OnRoundEnd() => Task.CompletedTask;

	/// <summary>
	/// Called on the host after <see cref="OnRoundEnd"/> completes.
	/// </summary>
	public void PostRoundEnd() { }
}

public interface ISpawnPointAssigner
{
	/// <summary>
	/// Select a spawn point for a player on the given team.
	/// </summary>
	Transform GetSpawnTransform( Team team );
}
