namespace Facepunch;

public abstract class WeaponModel : Component
{
	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }

	[Property]
	public GameObject Muzzle { get; set; }

	[Property]
	public GameObject EjectionPort { get; set; }

	public void Deploy()
	{
		ModelRenderer.Set( "b_deploy", true );
	}

}
