﻿using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Gives a special weapon to one player on the specified team at the start of this state.
/// </summary>
public sealed class SpecialWeaponAllocator : Component,
	IGameEventHandler<EnterStateEvent>
{
	/// <summary>
	/// We'll give this weapon to one player on the specified team.
	/// </summary>
	[Property]
	public EquipmentResource Weapon { get; set; }

	/// <summary>
	/// Which team to give the special weapon to.
	/// </summary>
	[Property]
	public TeamDefinition Team { get; set; }

	/// <summary>
	/// Only give equipment to players that spawned at a point with this tag.
	/// </summary>
	[Property]
	public string SpawnPointTag { get; set; }

	[After<RespawnPlayers>]
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( Weapon is null )
		{
			return;
		}

		var playersOnTeam = GameUtils.GetPlayerPawns( Team )
			.Where( x => string.IsNullOrEmpty( SpawnPointTag ) || x.SpawnPointTags.Contains( SpawnPointTag, StringComparer.OrdinalIgnoreCase ) )
			.Shuffle();

		if ( playersOnTeam.Count == 0 )
		{
			return;
		}

		Log.Info( $"Trying to spawn {Weapon} on {playersOnTeam[0]}" );

		var playerToGiveTo = playersOnTeam[0];

		// Conna: this is a special weapon for a specific team. Remove it for everyone.
		foreach ( var player in GameUtils.PlayerPawns )
		{
			player.Inventory.Remove( Weapon );
		}

		// Conna: now give it to that specific player only.
		var weapon = playerToGiveTo.Inventory.Give( Weapon, false );
		if ( weapon.IsValid() )
		{
			weapon.Components.GetOrCreate<DestroyBetweenRounds>();
		}
	}
}
