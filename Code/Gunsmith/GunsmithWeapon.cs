namespace Facepunch;

public sealed class GunsmithWeapon : Component
{
	[Property]
	public EquipmentResource Resource { get; set; }

	[Property]
	public SkinnedModelRenderer Renderer { get; set; }

	[RequireComponent]
	public ViewModel ViewModel { get; set; }
}
