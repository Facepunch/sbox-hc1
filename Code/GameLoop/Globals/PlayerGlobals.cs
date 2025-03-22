using Sandbox;
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// A list of globals that are relevant to the player. Health, armor, VFX that we don't want to hardcode somewhere.
/// </summary>
public class PlayerGlobals : GlobalComponent, IGameEventHandler<ModifyDamageGlobalEvent>
{
	public class HitboxConfig
	{
		public HitboxTags Tags { get; set; }
		public float DamageScale { get; set; } = 1f;
		public bool ArmorProtects { get; set; }
		public bool HelmetProtects { get; set; }
	}

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

	public const float DefaultArmorReduction = 0.775f;
	public const float DefaultHelmetReduction = 0.775f;

	/// <summary>
	/// How much should we scale damage by if the player is using armor?
	/// </summary>
	[Property, Group( "Damage" )] public float BaseArmorReduction { get; set; } = DefaultArmorReduction;

	/// <summary>
	/// How much should we scale damage by if the player is wearing a helmet?
	/// </summary>
	[Property, Group( "Damage" )] public float BaseHelmetReduction { get; set; } = DefaultHelmetReduction;

	/// <summary>
	/// How much spread should we have when aiming down the sights?
	/// </summary>
	[Property, Group( "Damage" )] public float AimSpread { get; set; } = 0f;

	/// <summary>
	/// How much spread do we apply when aiming WHEN moving?
	/// </summary>
	[Property, Group( "Damage" )] public float AimVelocitySpreadScale { get; set; } = 0.5f;

	/// <summary>
	/// The base amount of spread all weapons have.
	/// </summary>
	[Property, Group( "Spread" )] public float BaseSpreadAmount { get; set; } = 0.05f;

	/// <summary>
	/// What's our upper limit when applying spread based on velocity?
	/// </summary>
	[Property, Group( "Spread" )] public float SpreadVelocityLimit { get; set; } = 350f;

	/// <summary>
	/// How much spread should we add based on how fast the player is moving
	/// </summary>
	[Property, Group( "Spread" )] public float VelocitySpreadScale { get; set; } = 0.1f;

	/// <summary>
	/// How much spread should we scale by when we're crouching?
	/// </summary>
	[Property, Group( "Spread" )] public float CrouchSpreadScale { get; set; } = 0.5f;

	/// <summary>
	/// How much spread should we scale by when we're in the air?
	/// </summary>
	[Property, Group( "Spread" )] public float AirSpreadScale { get; set; } = 2.0f;

	public static List<HitboxConfig> GetDefaultHitboxConfigs()
	{
		return new()
		{
			new HitboxConfig { Tags = HitboxTags.Head, DamageScale = 5f, HelmetProtects = true },
			new HitboxConfig { Tags = HitboxTags.UpperBody | HitboxTags.Arm, ArmorProtects = true },
			new HitboxConfig { Tags = HitboxTags.LowerBody, DamageScale = 1.25f },
			new HitboxConfig { Tags = HitboxTags.Leg, DamageScale = 0.75f }
		};
	}

	/// <summary>
	/// Custom damage scales / armor info for each hitbox tag. Uses the first match in the list.
	/// </summary>
	[Property, Group( "Damage" )]
	public List<HitboxConfig> Hitboxes { get; set; } = GetDefaultHitboxConfigs();

	/// <summary>
	/// If true, pop off helmets on headshots.
	/// </summary>
	[Property, Group( "Damage" )]
	public bool RemoveHelmetOnHeadshot { get; set; } = true;

	/// <summary>
	/// The current gravity.
	/// </summary>
	[Property, Group( "Movement" )] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );
	[Property, Group( "Movement" )] public float JumpPower { get; set; } = 290f;

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
	[Property, Group( "Movement" )] public float AirAcceleration { get; set; } = 16f;
	[Property, Group( "Movement" )] public float BaseAcceleration { get; set; } = 9f;
	[Property, Group( "Movement" )] public float SlowWalkAcceleration { get; set; } = 10;
	[Property, Group( "Movement" )] public float CrouchingAcceleration { get; set; } = 10;
	[Property, Group( "Movement" )] public float SprintingAcceleration { get; set; } = 8f;

	// Acceleration
	[Property, Group( "Movement" )] public float MaxAcceleration { get; set; } = 10f;
	[Property, Group( "Movement" )] public float AirMaxAcceleration { get; set; } = 80f;

	// Speed
	[Property, Group( "Movement" )] public float WalkSpeed { get; set; } = 220f;
	[Property, Group( "Movement" )] public float SlowWalkSpeed { get; set; } = 100f;
	[Property, Group( "Movement" )] public float CrouchingSpeed { get; set; } = 100f;
	[Property, Group( "Movement" )] public float SprintingSpeed { get; set; } = 260f;

	// Friction
	[Property, Group( "Movement" )] public float WalkFriction { get; set; } = 7f;
	[Property, Group( "Movement" )] public float SlowWalkFriction { get; set; } = 4f;
	[Property, Group( "Movement" )] public float CrouchingFriction { get; set; } = 4f;
	[Property, Group( "Movement" )] public float SprintingFriction { get; set; } = 4f;

	// Crouch
	[Property, Group( "Movement" )] public float CrouchLerpSpeed { get; set; } = 10f;
	[Property, Group( "Movement" )] public float SlowCrouchLerpSpeed { get; set; } = 0.5f;

	// Damage Acceleration
	[Property, Group( "Damage Acceleration" )] public float TakeDamageAccelerationDampenTime { get; set; } = 1f;
	[Property, Group( "Damage Acceleration" )] public float TakeDamageAccelerationOffset { get; set; } = 0.5f;

	/// <summary>
	/// Apply armor and helmet damage modifications.
	/// </summary>
	[Early]
	void IGameEventHandler<ModifyDamageGlobalEvent>.OnGameEvent( ModifyDamageGlobalEvent eventArgs )
	{
		if ( eventArgs.DamageInfo.WasFallDamage && !EnableFallDamage )
		{
			eventArgs.ClearDamage();
			return;
		}

		var resource = (eventArgs.DamageInfo.Inflictor as Equipment)?.Resource;

		GetDamageModifications(
			eventArgs.DamageInfo.Flags, eventArgs.DamageInfo.Hitbox,
			resource?.ArmorReduction ?? BaseArmorReduction,
			resource?.HelmetReduction ?? BaseHelmetReduction,
			Hitboxes,
			out var damageScale, out var armorReduction, out var removeHelmet );

		eventArgs.ScaleDamage( damageScale );
		eventArgs.ApplyArmor( armorReduction );

		if ( removeHelmet )
		{
			eventArgs.RemoveHelmet();
		}
	}

	public static void GetDamageModifications(
		DamageFlags damageFlags,
		HitboxTags hitboxTags,
		float baseArmorReduction,
		float baseHelmetReduction,
		IReadOnlyList<HitboxConfig> hitboxConfigs,
		out float damageScale,
		out float armorReduction,
		out bool removeHelmet )
	{
		damageScale = 1f;
		armorReduction = 1f;
		removeHelmet = false;

		if ( hitboxConfigs.FirstOrDefault( x => (x.Tags & hitboxTags) != 0 ) is not { } config )
		{
			// We don't have any special rules for this hitbox
			return;
		}

		damageScale = config.DamageScale;

		if ( config.HelmetProtects && (damageFlags & DamageFlags.Helmet) != 0 )
		{
			armorReduction = baseHelmetReduction;
			removeHelmet = true;
		}
		else if ( config.ArmorProtects && (damageFlags & DamageFlags.Armor) != 0 )
		{
			armorReduction = baseArmorReduction;
		}
	}
}
