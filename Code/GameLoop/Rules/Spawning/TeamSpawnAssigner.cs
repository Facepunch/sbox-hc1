using Facepunch;
using Sandbox.Events;

/// <summary>
/// Assign a spawn point for each player at the start of each round, based on their team.
/// </summary>
public sealed class TeamSpawnAssigner : Component,
	IGameEventHandler<PreRoundStartEvent>,
	ISpawnAssigner
{
	/// <summary>
	/// Use spawns that include at least one of these tags.
	/// </summary>
	[Property, Title( "Tags" )]
	public List<string> SpawnTags { get; private set; } = new();

	private Dictionary<PlayerController, Transform> AssignedSpawns { get; } = new();

	[Before<RoundStartPlayerSpawner>]
	void IGameEventHandler<PreRoundStartEvent>.OnGameEvent( PreRoundStartEvent eventArgs )
	{
		AssignedSpawns.Clear();

		foreach ( var team in GameUtils.Teams )
		{
			var spawns = GameUtils.GetSpawnPoints( team, Tags.ToArray() ).Shuffle();

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
