using System.Threading.Tasks;
using Facepunch;

public sealed class RoundStartPlayerSpawner : Component, IRoundStartListener
{
	public Task OnRoundStart()
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.Respawn();
		}

		return Task.CompletedTask;
	}
}
