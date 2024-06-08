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

	/// <summary>
	/// How much should we scale damage by if the player is using armor?
	/// </summary>
	[Property, Group( "Damage" )] public float BaseArmorReduction { get; set; } = 0.775f;

	/// <summary>
	/// How much should we scale the damage by if the player is hit in certain areas?
	/// </summary>
	[Property, Group( "Damage" )]
	Dictionary<string, float> HitboxDamage { get; set; } = new()
	{
		{ "head helmet", 1f },
		{ "head", 5f },
	};

	/// <summary>
	/// Safe accessor for <see cref="HitboxDamage"/>
	/// </summary>
	/// <param name="hitbox"></param>
	/// <returns></returns>
	public float GetDamageMultiplier( string hitbox )
	{
		if ( HitboxDamage.TryGetValue( hitbox, out var mult ) )
		{
			return mult;
		}
		return 1f;
	}

	/// <summary>
	/// The current gravity.
	/// </summary>
	[Property, Group( "Movement" )] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	/// <summary>
	/// Can we bunny hop?
	/// </summary>
	[Property, Group( "Movement" )] public bool BunnyHopping { get; set; } = false;

	/// <summary>
	/// Is fall damage enabled?
	/// </summary>
	[Property, Group( "Movement" )] public bool EnableFallDamage { get; set; } = true;

	/// <summary>
	/// Can we control our movement in the air?
	/// </summary>
	[Property, Group( "Movement" )] public float AirAcceleration { get; set; } = 40f;
	[Property, Group( "Movement" )] public float BaseAcceleration { get; set; } = 10;
	[Property, Group( "Movement" )] public float SlowWalkAcceleration { get; set; } = 10;
	[Property, Group( "Movement" )] public float CrouchingAcceleration { get; set; } = 10;

	[Property, Group( "Movement" )] public float MaxAcceleration { get; set; } = 100f;
	[Property, Group( "Movement" )] public float AirMaxAcceleration { get; set; } = 150f;
}
