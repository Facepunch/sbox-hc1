
using Gunfight;

[GameResource( "Gunfight/Weapon Data", "weapon", "A resource containing basic information about a weapon.", IconBgColor = "#5877E0", Icon = "track_changes" )]
public partial class WeaponDataResource : GameResource
{
	public static HashSet<WeaponDataResource> All { get; set; } = new();

	[Category( "Base" )]
	public string Name { get; set; } = "My Weapon";
	
	[Category( "Base" )]
	public string Description { get; set; } = "";

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
