using Sandbox.Events;

namespace Facepunch;

public class MountPoint
{
	[KeyProperty] public WeaponSlot Slot { get; set; }
	[KeyProperty] public List<GameObject> GameObjects { get; set; } = new();
}

public sealed class EquipmentHolsters : Component,
	IGameEventHandler<WeaponDeployedEvent>,
	IGameEventHandler<WeaponHolsteredEvent>
{
	[Property] public List<MountPoint> MountPoints { get; set; } = new();

	public MountPoint GetMount( Weapon weapon )
	{
		var mount = MountPoints.FirstOrDefault( x => x.Slot == weapon.Resource.Slot );

		return mount;
	}

	void IGameEventHandler<WeaponDeployedEvent>.OnGameEvent( WeaponDeployedEvent eventArgs )
	{
		var newParent = eventArgs.Weapon?.PlayerController?.Inventory?.WeaponGameObject;
		if ( !newParent.IsValid() )
			return;

		eventArgs.Weapon.GameObject.SetParent( newParent, false );
	}

	void IGameEventHandler<WeaponHolsteredEvent>.OnGameEvent( WeaponHolsteredEvent eventArgs )
	{
		var mount = GetMount( eventArgs.Weapon );
		if ( mount is null )
			return;

		eventArgs.Weapon.ModelRenderer.Enabled = true;
		eventArgs.Weapon.GameObject.SetParent( mount.GameObjects.FirstOrDefault(), false );
		eventArgs.Weapon.GameObject.Transform.ClearInterpolation();
	}
}
