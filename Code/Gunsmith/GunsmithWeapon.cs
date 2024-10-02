namespace Facepunch;

public sealed class GunsmithWeapon : Component
{
	[Property]
	public EquipmentResource Resource { get; set; }

	[Property] 
	public GameObject RawViewModelPrefab { get; set; }

	[Property]
	public SkinnedModelRenderer Renderer { get; set; }

	protected override void OnStart()
	{
		var inst = RawViewModelPrefab.Clone( new CloneConfig()
		{
			 Name = "Runtime Weapon",
			 Parent = GameObject,
			 StartEnabled = true,
			 Transform = new()
		} );

		Renderer = inst.GetComponent<SkinnedModelRenderer>();
		Renderer.UseAnimGraph = false;
	}
}
