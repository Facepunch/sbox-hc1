﻿using Facepunch;

public sealed class DefaultEquipment : Component, IPlayerSpawnListener
{
	/// <summary>
	/// A weapon set that we'll give the player when they spawn.
	/// </summary>
	[Property] public List<WeaponDataResource> Weapons { get; set; }

	void IPlayerSpawnListener.PrePlayerSpawn( PlayerController player )
	{
		if ( Weapons is null ) return;

		if ( player.Inventory.Weapons.Count > 0 ) return;

		foreach ( var weapon in Weapons )
		{
			player.Inventory.GiveWeapon( weapon );
		}
	}
}