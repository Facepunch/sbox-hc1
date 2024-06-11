namespace Facepunch;

public partial class EquippmentDropper : Component, IKillListener
{
	[RequireComponent] DefaultEquipment DefaultEquipment { get; set; }

	/// <summary>
	/// Can we drop this weapon?
	/// </summary>
	/// <param name="weapon"></param>
	/// <returns></returns>
	private bool CanDrop( Weapon weapon )
	{
		if ( DefaultEquipment.Weapons.Contains( weapon.Resource ) )
			return false;

		if ( weapon.Resource.Slot == WeaponSlot.Melee ) 
			return false;

		return true;
	}

	void IKillListener.OnPlayerKilled( DamageEvent damageEvent )
	{
		var player = GameUtils.GetPlayerFromComponent( damageEvent.Victim );
		if ( !player.IsValid() )
			return;

		var droppables = player.Inventory.Weapons
			.Where( CanDrop )
			.ToList();

		for ( var i = droppables.Count - 1; i >= 0; i-- )
		{
			player.Inventory.DropWeapon( droppables[i].Id );
		}

		player.Inventory.Clear();
	}
}
