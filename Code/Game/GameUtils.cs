namespace Facepunch;

/// <summary>
/// A list of game utilities that'll help us achieve common goals with less code... I guess?
/// </summary>
public partial class GameUtils
{
	public static IEnumerable<SpawnPoint> GetSpawnPoints()
	{
		return Game.ActiveScene.GetAllComponents<SpawnPoint>();
	}
}
