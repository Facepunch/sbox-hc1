﻿using Sandbox.Events;

namespace Facepunch;

public class SpawnRule
{
	public TeamDefinition Team { get; set; }
	public string Tag { get; set; }

	public int MinPlayers { get; set; }
	public int MaxPlayers { get; set; }
}

/// <summary>
/// Use team-specific spawn points.
/// </summary>
public sealed class TeamSpawnAssigner : Component,
	ISpawnAssigner
{
	/// <summary>
	/// Use spawns that include at least one of these tags.
	/// </summary>
	[Property, Title( "Tags" )]
	public TagSet SpawnTags { get; private set; } = new();

	[Property, InlineEditor]
	public List<SpawnRule> SpawnRules { get; private set; } = new();

	public SpawnPointInfo GetSpawnPoint( PlayerState player )
	{
		var team = player.Team;
		var spawns = GameUtils.GetSpawnPoints( team, SpawnTags.ToArray() ).Shuffle();

		if ( spawns.Count == 0 && player.Team is not null )
		{
			Log.Warning( $"No spawn points for team {team}!" );
			return GameUtils.GetRandomSpawnPoint( null );
		}

		var playerPositions = GameUtils.PlayerPawns
			.Where( x => x.PlayerState != player )
			.Where( x => x.TimeSinceLastRespawn < 1f )
			.Where( x => x.HealthComponent.State == LifeState.Alive )
			.Select( x => (x.Transform.Position, Tags: x.SpawnPointTags) )
			.ToArray();

		foreach ( var rule in SpawnRules )
		{
			if ( rule.Team != player.Team )
			{
				continue;
			}

			var matchingPlayers = playerPositions
				.Count( x => x.Tags.Contains( rule.Tag, StringComparer.OrdinalIgnoreCase ) );

			if ( matchingPlayers >= rule.MaxPlayers )
			{
				continue;
			}

			if ( matchingPlayers < rule.MinPlayers )
			{
				spawns = spawns
					.Where( x => x.Tags.Contains( rule.Tag, StringComparer.OrdinalIgnoreCase ) )
					.ToArray();
			}
		}

		foreach ( var spawn in spawns )
		{
			if ( playerPositions.All( x => (x.Position - spawn.Position).LengthSquared > 32f * 32f ) )
			{
				return spawn;
			}
		}

		return spawns[0];
	}
}
