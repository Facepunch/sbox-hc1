namespace Facepunch;

/// <summary>
/// Interface for components on weapons that persist when dropped.
/// </summary>
public interface IDroppedWeaponState
{
	void CopyToDroppedWeapon( DroppedEquipment dropped );
	void CopyFromDroppedWeapon( DroppedEquipment dropped );
}

/// <summary>
/// Interface for components on weapons that persist when dropped.
/// Default implementation will create a copy of this component on the dropped weapon, then copy it back on pickup.
/// </summary>
public interface IDroppedWeaponState<T> : IDroppedWeaponState
	where T : Component, IDroppedWeaponState<T>, new()
{
	void IDroppedWeaponState.CopyToDroppedWeapon( DroppedEquipment dropped )
	{
		var state = dropped.Components.GetOrCreate<T>();

		((T)this).CopyPropertiesTo( state );
	}

	void IDroppedWeaponState.CopyFromDroppedWeapon( DroppedEquipment dropped )
	{
		if ( dropped.Components.Get<T>() is {} state )
		{
			state.CopyPropertiesTo( (T) this );
		}
	}
}
