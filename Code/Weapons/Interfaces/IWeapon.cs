namespace Facepunch;

public interface IWeapon : IValid
{
	public SkinnedModelRenderer ModelRenderer { get; set; }
	public GameObject Muzzle { get; set; }
	public GameObject EjectionPort { get; set; }
	public GameObject GameObject { get; }
}
