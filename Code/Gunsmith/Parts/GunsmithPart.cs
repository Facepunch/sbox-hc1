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

	/// <summary>
	/// Which weapons does this part support? By default, if it's empty, it's universal
	/// </summary>
	[Property]
	public List<EquipmentResource> EligibleEquipment { get; set; } = new();

	[Property]
	public bool ModifyBodygroups { get; set; } = false;

	[Property, ShowIf( nameof( ModifyBodygroups ), true )]
	public Dictionary<string, int> Bodygroups { get; set; } = new();

	/// <summary>
	/// Is this a universal attachment?
	/// </summary>
	public bool IsUniversal => EligibleEquipment.Count() < 1;

	public bool Supports( EquipmentResource resource )
	{
		if ( IsUniversal ) return true;
		return EligibleEquipment.Contains( resource );
	}
}
