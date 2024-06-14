using Sandbox.Events;

namespace Facepunch;

public class MountPoint
{
	[KeyProperty] public EquipmentSlot Slot { get; set; }
	[KeyProperty] public List<GameObject> GameObjects { get; set; } = new();
}

public sealed class EquipmentHolsters : Component,
	IGameEventHandler<EquipmentDeployedEvent>,
	IGameEventHandler<EquipmentHolsteredEvent>
{
	[Property] public List<MountPoint> MountPoints { get; set; } = new();

	public MountPoint GetMount( Equipment equipment )
	{
		var mount = MountPoints.FirstOrDefault( x => x.Slot == equipment.Resource.Slot );

		return mount;
	}

	void IGameEventHandler<EquipmentDeployedEvent>.OnGameEvent( EquipmentDeployedEvent eventArgs )
	{
		var newParent = eventArgs.Equipment?.PlayerController?.Inventory?.WeaponGameObject;
		if ( !newParent.IsValid() )
			return;

		eventArgs.Equipment.GameObject.SetParent( newParent, false );
	}

	void IGameEventHandler<EquipmentHolsteredEvent>.OnGameEvent( EquipmentHolsteredEvent eventArgs )
	{
		var mount = GetMount( eventArgs.Equipment );
		if ( mount is null )
			return;

		eventArgs.Equipment.ModelRenderer.Enabled = true;
		eventArgs.Equipment.GameObject.SetParent( mount.GameObjects.FirstOrDefault(), false );
		eventArgs.Equipment.GameObject.Transform.ClearInterpolation();
	}
}
