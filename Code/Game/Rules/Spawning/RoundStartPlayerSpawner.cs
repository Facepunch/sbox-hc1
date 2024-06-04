using System.Threading.Tasks;
using Facepunch;

/// <summary>
/// Respawn all players at the start of each round.
/// </summary>
public sealed class RoundStartPlayerSpawner : Component, IRoundStartListener
{
	Task IRoundStartListener.OnRoundStart()
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			if ( GameMode.Instance.Components.GetInDescendantsOrSelf<ISpawnAssigner>() is { } spawnAssigner )
			{
				player.Teleport( spawnAssigner.GetSpawnPoint( player ) );
			}

			player.Respawn();
		}

		return Task.CompletedTask;
	}
}
