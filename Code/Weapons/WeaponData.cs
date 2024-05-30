
using Facepunch;

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

[GameResource( "hc1/Weapon Data", "wpn", "A resource containing basic information about a weapon.", IconBgColor = "#5877E0", Icon = "track_changes" )]
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
	/// If set, only this team can buy the weapon.
	/// </summary>
	[Category( "Base" )]
	public Team Team { get; set; }

	/// <summary>
	/// If false, only <see cref="Team"/> can pick up this weapon.
	/// </summary>
	[Category( "Base" ), HideIf( nameof(Team), Team.Unassigned )]
	public bool CanOtherTeamPickUp { get; set; } = true;

	/// <summary>
	/// The weapon's icon
	/// </summary>
	[Group( "Base" ), ImageAssetPath] public string Icon { get; set; }

	/// <summary>
	/// Is this weapon shown in the buy menu
	/// </summary>
	[Category("Economy")] public bool IsPurchasable { get; set; } = true;

	/// <summary>
	/// How much is this weapon to buy in the buy menu?
	/// </summary>
	[Category( "Economy" )] public int Price { get; set; } = 0;

	/// <summary>
	/// How much money do you get per kill with this weapon?
	/// </summary>
	[Category( "Economy" )] public int KillReward { get; set; } = 300;

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

	public bool IsPurchasableForTeam( Team team )
	{
		return Team == Team.Unassigned || Team == team;
	}

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
