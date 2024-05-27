using System.Threading.Tasks;
using Facepunch;

/// <summary>
/// Respawn all players at the start of each round.
/// </summary>
public sealed class RoundStartPlayerSpawner : Component, IRoundStartListener
{
	void IRoundStartListener.PreRoundStart()
	{
		foreach (var player in GameUtils.ActivePlayers)
		{
			player.Respawn();
		}
	}
}
