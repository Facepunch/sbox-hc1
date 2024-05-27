using System.Threading.Tasks;
using Facepunch;

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
