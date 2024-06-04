using Facepunch;

/// <summary>
/// Place players at spawn points matching their teams.
/// </summary>
public sealed class TeamSpawnAssigner : Component, IRoundStartListener, ISpawnAssigner
{
	private Dictionary<PlayerController, Transform> AssignedSpawns { get; } = new();

	void IRoundStartListener.PreRoundStart()
	{
		AssignedSpawns.Clear();

		foreach ( var team in GameUtils.Teams )
		{
			var spawns = GameUtils.GetSpawnPoints( team ).Shuffle();

			if ( spawns.Count < 1 )
			{
				Log.Warning( $"No spawns for team {team}!" );
				continue;
			}

			var players = GameUtils.GetPlayers( team ).ToArray();

			if ( spawns.Count < players.Length )
			{
				Log.Warning( $"Not enough spawns for team {team}! Need {players.Length}, found {spawns.Count}." );
			}

			for ( var i = 0; i < players.Length; ++i )
			{
				AssignedSpawns[players[i]] = spawns[i % spawns.Count];
			}
		}
	}

	public Transform GetSpawnPoint( PlayerController player )
	{
		return AssignedSpawns.TryGetValue( player, out var spawn )
			? spawn
			: GameUtils.GetSpawnPoints( player.TeamComponent.Team ).Shuffle().First();
	}
}
