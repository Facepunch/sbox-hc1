namespace Facepunch.Gunsmith;

public enum GunsmithPartType
{
	Charm,
	Camoflague,
	Barrel,
	Stock,
	Underbarrel,
	Optic
}

[GameResource( "HC1/Gunsmith Part", "gnsmth", "A gunsmith part" )]
public partial class GunsmithPart : GameResource
{
	/// <summary>
	/// What's this part called?
	/// </summary>
	[Property]
	public string Name { get; set; }

	/// <summary>
	/// The category for this part
	/// </summary>
	[Property]
	public GunsmithPartType Category { get; set; }

	/// <summary>
	/// Prefab!
	/// </summary>
	[Property] 
	public GameObject MainPrefab { get; set; }
}
