namespace Facepunch;

/// <summary>
/// A list of globals that are relevant to the player. Health, armor, VFX that we don't want to hardcode somewhere.
/// </summary>
public class PlayerGlobals : GlobalComponent
{
	/// <summary>
	/// What's the player's max HP?
	/// </summary>
	[Property, Group( "Health" )] public float MaxHealth { get; private set; } = 100f;

	/// <summary>
	/// What's the player's max armor?
	/// </summary>
	[Property, Group( "Health" )] public float MaxArmor { get; private set; } = 100f;

	/// <summary>
	/// What decals should we use for blood impacts?
	/// </summary>
	[Property, Group( "Effects" )] public List<Material> BloodDecalMaterials { get; set; } = new()
	{
		Cloud.Material( "jase.bloodsplatter08" ),
		Cloud.Material( "jase.bloodsplatter07" ),
		Cloud.Material( "jase.bloodsplatter06" ),
		Cloud.Material( "jase.bloodsplatter05" ),
		Cloud.Material( "jase.bloodsplatter04" )
	};
}
