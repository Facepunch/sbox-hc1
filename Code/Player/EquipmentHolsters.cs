namespace Facepunch;

public class MountPoint
{
	[KeyProperty] public WeaponSlot Slot { get; set; }
	[KeyProperty] public List<GameObject> GameObjects { get; set; } = new();
}

public sealed class EquipmentHolsters : Component, Weapon.IDeploymentListener
{
	[Property] public List<MountPoint> MountPoints { get; set; } = new();

	public MountPoint GetMount( Weapon weapon )
	{
		var mount = MountPoints.FirstOrDefault( x => x.Slot == weapon.Resource.Slot );

		return mount;
	}

	void Weapon.IDeploymentListener.OnDeployed( Weapon weapon )
	{
		var newParent = weapon?.PlayerController?.Inventory?.WeaponGameObject;
		if ( !newParent.IsValid() )
			return;

		weapon.GameObject.SetParent( newParent, false );
	}

	void Weapon.IDeploymentListener.OnHolstered( Weapon weapon )
	{
		var mount = GetMount( weapon );
		if ( mount is null )
			return;

		weapon.ModelRenderer.Enabled = true;
		weapon.GameObject.SetParent( mount.GameObjects.FirstOrDefault(), false );
		weapon.GameObject.Transform.ClearInterpolation();
	}
}
