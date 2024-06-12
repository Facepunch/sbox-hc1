using Sandbox.Events;

public sealed class DefaultEquipment : Component,
	IGameEventHandler<PlayerSpawnedEvent>
{
	/// <summary>
	/// A weapon set that we'll give the player when they spawn.
	/// </summary>
	[Property] public List<WeaponData> Weapons { get; set; }

	[Property] public int Armor { get; set; }
	[Property] public bool Helmet { get; set; }
	[Property] public bool RefillAmmo { get; set; } = true;

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		if ( Weapons is null ) return;

		var player = eventArgs.Player;

		foreach ( var weapon in Weapons )
		{
			if ( !player.Inventory.HasWeaponInSlot( weapon.Slot ) )
				player.Inventory.GiveWeapon( weapon, false );
		}

		player.Inventory.SwitchToBestWeapon();

		player.HealthComponent.Armor = Math.Max( player.HealthComponent.Armor, Armor );
		player.HealthComponent.HasHelmet |= Helmet;

		if ( RefillAmmo )
		{
			player.Inventory.RefillAmmo();
		}
	}
}
