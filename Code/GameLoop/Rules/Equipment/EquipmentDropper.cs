namespace Facepunch;

public partial class EquippmentDropper : Component, IKillListener
{
	void IKillListener.OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force, Component inflictor, string hitbox = "" )
	{
		var player = GameUtils.GetPlayerFromComponent( victim );
		if ( !player.IsValid() )
			return;

		var specials = player.Inventory.Weapons
			.Where( x => x.Resource.Slot == WeaponSlot.Special )
			.ToList();

		for ( var i = specials.Count - 1; i >= 0; i-- )
		{
			player.Inventory.DropWeapon( specials[i].Id );
		}

		var currentWeapon = player.Inventory.CurrentWeapon;

		if ( currentWeapon.IsValid() && currentWeapon.Resource.Slot != WeaponSlot.Melee )
		{
			player.Inventory.DropWeapon( currentWeapon.Id );
		}

		player.Inventory.Clear();
	}
}
