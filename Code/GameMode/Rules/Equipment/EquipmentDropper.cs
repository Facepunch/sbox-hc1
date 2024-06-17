using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Players drop their held weapon when killed.
/// </summary>
public partial class EquipmentDropper : Component,
	IGameEventHandler<KillEvent>
{
	/// <summary>
	/// Which categories can we drop?
	/// </summary>
	[Property] public List<EquipmentSlot> Categories { get; set; } = new();

	/// <summary>
	/// Can we drop this weapon?
	/// </summary>
	/// <param name="weapon"></param>
	/// <returns></returns>
	private bool CanDrop( Equipment weapon )
	{
		if ( GameMode.Instance.Get<DefaultEquipment>()?.Weapons.Contains( weapon.Resource ) is true )
			return false;

		if ( weapon.Resource.Slot == EquipmentSlot.Melee ) 
			return false;

		if ( Categories.Count == 0 ) return true;

		return Categories.Contains( weapon.Resource.Slot );
	}

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		var player = GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Victim );
		if ( !player.IsValid() )
			return;

		var droppables = player.Inventory.Equipment
			.Where( CanDrop )
			.ToList();

		for ( var i = droppables.Count - 1; i >= 0; i-- )
		{
			player.Inventory.Drop( droppables[i].Id );
		}

		if ( Categories.Count < 1 )
			player.Inventory.Clear();
	}
}
