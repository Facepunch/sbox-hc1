namespace Facepunch;

/// <summary>
/// Interface for components on weapons that persist when dropped.
/// </summary>
public interface IDroppedWeaponState
{
	void CopyToDroppedWeapon( DroppedWeapon dropped );
	void CopyFromDroppedWeapon( DroppedWeapon dropped );
}

/// <summary>
/// Interface for components on weapons that persist when dropped.
/// Default implementation will create a copy of this component on the dropped weapon, then copy it back on pickup.
/// </summary>
public interface IDroppedWeaponState<T> : IDroppedWeaponState
	where T : Component, IDroppedWeaponState<T>, new()
{
	void IDroppedWeaponState.CopyToDroppedWeapon( DroppedWeapon dropped )
	{
		var state = dropped.Components.GetOrCreate<T>();

		((T)this).CopyPropertiesTo( state );
	}

	void IDroppedWeaponState.CopyFromDroppedWeapon( DroppedWeapon dropped )
	{
		if ( dropped.Components.Get<T>() is {} state )
		{
			state.CopyPropertiesTo( (T) this );
		}
	}
}
