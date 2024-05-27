
using System.Threading.Tasks;
using Facepunch;

public interface IGameEndCondition
{
	/// <summary>
	/// Called at the end of a round to determine if the game should end.
	/// </summary>
	bool ShouldGameEnd();
}

public interface IRoundEndCondition
{
	/// <summary>
	/// Called every update during a round to determine if it should end.
	/// </summary>
	bool ShouldRoundEnd();
}

public interface IGameStartListener
{
	public void PreGameStart() { }
	public Task OnGameStart() => Task.CompletedTask;
	public void PostGameStart() { }
}

public interface IGameEndListener
{
	public void PreGameEnd() { }
	public Task OnGameEnd() => Task.CompletedTask;
	public void PostGameEnd() { }
}

public interface IRoundStartListener
{
	public void PreRoundStart() { }
	public Task OnRoundStart() => Task.CompletedTask;
	public void PostRoundStart() { }
}

public interface IRoundEndListener
{
	public void PreRoundEnd() { }
	public Task OnRoundEnd() => Task.CompletedTask;
	public void PostRoundEnd() { }
}

public interface ISpawnPointAssigner
{
	Transform GetSpawnTransform( Team team );
}
