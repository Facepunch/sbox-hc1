
public enum WeaponSlot
{
	Undefined = 0,
	
	/// <summary>
	/// Non-pistol guns.
	/// </summary>
	Primary = 1,

	/// <summary>
	/// Pistols.
	/// </summary>
	Secondary = 2,

	/// <summary>
	/// Knives etc.
	/// </summary>
	Melee = 3,

	/// <summary>
	/// Grenades etc.
	/// </summary>
	Utility = 4,

	/// <summary>
	/// C4 etc.
	/// </summary>
	Special = 5
}

[GameResource( "Gunfight/Weapon Data", "wpn", "A resource containing basic information about a weapon.", IconBgColor = "#5877E0", Icon = "track_changes" )]
public partial class WeaponData : GameResource
{
	public static HashSet<WeaponData> All { get; set; } = new();

	[Category( "Base" )]
	public string Name { get; set; } = "My Weapon";
	
	[Category( "Base" )]
	public string Description { get; set; } = "";

	[Category( "Base" )]
	public WeaponSlot Slot { get; set; }

	/// <summary>
	/// The weapon's icon
	/// </summary>
	[Group( "Base" ), ImageAssetPath] public string Icon { get; set; }

	/// <summary>
	/// How much is this weapon to buy in the buy menu?
	/// </summary>
	[Category( "Economy" )] public int Price { get; set; } = 0;

	/// <summary>
	/// The prefab to create and attach to the player when spawning it in.
	/// </summary>
	[Category( "Prefabs" )]
	public GameObject MainPrefab { get; set; }

	/// <summary>
	/// The prefab to create when making a viewmodel for this weapon.
	/// </summary>
	[Category( "Prefabs" )]
	public GameObject ViewModelPrefab { get; set; }

	/// <summary>
	/// The weapon's model
	/// </summary>
	[Category( "Information" )]
	public Model WorldModel { get; set; }

	protected override void PostLoad()
	{
		if ( All.Contains( this ) )
		{
			Log.Warning( "Tried to add two of the same weapon (?)" );
			return;
		}

		All.Add( this );
	}
}
