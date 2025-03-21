using Facepunch;

/// <summary>
/// What slot is this equipment for?
/// </summary>
public enum EquipmentSlot
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

/// <summary>
/// A resource definition for a piece of equipment. This could be a weapon, or a deployable, or a gadget, or a grenade.. Anything really.
/// </summary>
[GameResource( "HC1/Equipment Item", "equip", "", IconBgColor = "#5877E0", Icon = "track_changes" )]
public partial class EquipmentResource : GameResource
{
	public static HashSet<EquipmentResource> All { get; set; } = new();

	[Category( "Base" )]
	public string Name { get; set; } = "My Equipment";
	
	[Category( "Base" )]
	public string Description { get; set; } = "";

	[Category( "Base" )]
	public EquipmentSlot Slot { get; set; }

	/// <summary>
	/// If set, only this team can buy the equipment.
	/// </summary>
	[Category( "Base" )]
	public Team Team { get; set; }

	/// <summary>
	/// If false, only <see cref="Team"/> can pick up this equipment.
	/// </summary>
	[Category( "Base" ), HideIf( nameof(Team), Team.Unassigned )]
	public bool CanOtherTeamPickUp { get; set; } = true;

	/// <summary>
	/// If true, owner will drop this equipment if they disconnect.
	/// </summary>
	[Category( "Base" )]
	public bool DropOnDisconnect { get; set; } = false;

	/// <summary>
	/// The equipment's icon
	/// </summary>
	[Group( "Base" ), ImageAssetPath] public string Icon { get; set; }

	/// <summary>
	/// Is this equipment shown in the buy menu
	/// </summary>
	[Category( "Economy" )] public bool IsPurchasable { get; set; } = true;

	/// <summary>
	/// How much is this equipment to buy in the buy menu?
	/// </summary>
	[Category( "Economy" )] public int Price { get; set; } = 0;

	/// <summary>
	/// How much money do you get per kill with this equipment?
	/// </summary>
	[Category( "Economy" )] public int KillReward { get; set; } = 300;

	/// <summary>
	/// The prefab to create and attach to the player when spawning it in.
	/// </summary>
	[Category( "Prefabs" )]
	public GameObject MainPrefab { get; set; }

	/// <summary>
	/// A world model that we'll put on the player's arms in third person.
	/// </summary>
	[Category( "Prefabs" )]
	public GameObject WorldModelPrefab { get; set; }

	/// <summary>
	/// The prefab to create when making a viewmodel for this equipment.
	/// </summary>
	[Category( "Prefabs" )]
	public GameObject ViewModelPrefab { get; set; }

	/// <summary>
	/// The equipment's model
	/// </summary>
	[Category( "Information" )]
	public Model WorldModel { get; set; }

	[Category( "Damage" )]
	public float? ArmorReduction { get; set; }

	[Category( "Damage" )]
	public float? HelmetReduction { get; set; }

	public bool IsPurchasableForTeam( Team team )
	{
		return Team == Team.Unassigned || Team == team;
	}

	protected override void PostLoad()
	{
		if ( All.Contains( this ) )
		{
			Log.Warning( "Tried to add two of the same equipment (?)" );
			return;
		}

		All.Add( this );
	}
}
