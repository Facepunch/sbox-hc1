namespace Facepunch;

[Title( "Molotov Fire Node" )]
public class MolotovFireNode : Component
{
	public static List<MolotovFireNode> All { get; set; } = new();

	public void Extinguish()
	{
		GameObject.Destroy();
	}
	
	protected override void OnEnabled()
	{
		All.Add( this );
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		All.Remove( this );
		base.OnDisabled();
	}
}
