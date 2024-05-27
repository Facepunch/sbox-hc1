
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
	Task OnGameStart();
}

public interface IGameEndListener
{
	Task OnGameEnd();
}

public interface IRoundStartListener
{
	Task OnRoundStart();
}

public interface IRoundEndListener
{
	Task OnRoundEnd();
}

public interface ISpawnPointAssigner
{
	Transform GetSpawnTransform( Team team );
}
