using System.Threading.Tasks;
using Facepunch;

public sealed class DefaultEquipment : Component, IPlayerSpawnListener
{
	/// <summary>
	/// A weapon set that we'll give the player when they spawn.
	/// </summary>
	[Property] public List<WeaponData> Weapons { get; set; }

	Task IPlayerSpawnListener.OnPlayerSpawn( PlayerController player )
	{
		if ( Weapons is null ) return Task.CompletedTask;

		Log.Info( $"PrePlayerSpawn for {player}" );

		foreach ( var weapon in Weapons )
		{
			if ( !player.Inventory.HasWeapon( weapon.Slot ) )
				player.Inventory.GiveWeapon( weapon, false );
		}

		player.Inventory.SwitchToBestWeapon();

		return Task.CompletedTask;
	}
}
