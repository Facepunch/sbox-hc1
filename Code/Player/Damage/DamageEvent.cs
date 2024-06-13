namespace Facepunch;

[Flags]
public enum HitboxTags
{
	None = 0,
	Head = 1,
	Chest = 2,
	Stomach = 4,
	Clavicle = 8,
	Arm = 16,
	Hand = 32,
	Leg = 64,
	Ankle = 128,
	Spine = 256,
	Neck = 512,

	UpperBody = Neck | Chest | Clavicle,
	LowerBody = Stomach
}

[Flags]
public enum DamageFlags
{
	None = 0,

	/// <summary>
	/// The victim was wearing kevlar.
	/// </summary>
	Armor = 1,

	/// <summary>
	/// The victim was wearing a helmet.
	/// </summary>
	Helmet = 2,

	/// <summary>
	/// This was a knife attack.
	/// </summary>
	Melee = 4,

	/// <summary>
	/// This was some kind of explosion.
	/// </summary>
	Explosion = 8,

	/// <summary>
	/// The victim fell.
	/// </summary>
	FallDamage = 16
}

/// <summary>
/// Event dispatched when a <see cref="HealthComponent"/> takes damage.
/// </summary>
/// <param name="Attacker">Who was the attacker?</param>
/// <param name="Damage">How much damage?</param>
/// <param name="Inflictor">What caused this damage? Can be a weapon, grenade, etc.</param>
/// <param name="Position">The point of the damage. Normally where you were hit.</param>
/// <param name="Force">The force of the damage.</param>
/// <param name="Hitbox">What hitbox did we hit?</param>
/// <param name="Flags">Extra data that we can pass around. Like if it's a blind-shot, mid-air shot, through smoke shot, etc.</param>
/// <param name="ArmorDamage">How much armor damage?</param>
/// <param name="RemoveHelmet">Did this damage remove the victim's helmet?</param>
public record DamageInfo( Component Attacker, float Damage, Component Inflictor = null,
	Vector3 Position = default, Vector3 Force = default,
	HitboxTags Hitbox = default, DamageFlags Flags = DamageFlags.None,
	float ArmorDamage = 0f, bool RemoveHelmet = false )
{
	/// <summary>
	/// Who took damage?
	/// </summary>
	public Component Victim { get; init; }

	/// <inheritdoc cref="DamageFlags.Armor"/>
	public bool HasArmor => Flags.HasFlag( DamageFlags.Armor );

	/// <inheritdoc cref="DamageFlags.Helmet"/>
	public bool HasHelmet => Flags.HasFlag( DamageFlags.Helmet );

	/// <inheritdoc cref="DamageFlags.Helmet"/>
	public bool WasMelee => Flags.HasFlag( DamageFlags.Melee );

	/// <inheritdoc cref="DamageFlags.Explosion"/>
	public bool WasExplosion => Flags.HasFlag( DamageFlags.Explosion );

	/// <inheritdoc cref="DamageFlags.FallDamage"/>
	public bool WasFallDamage => Flags.HasFlag( DamageFlags.FallDamage );

	public override string ToString()
	{
		return $"\"{Attacker}\" - \"{Victim}\" with \"{Inflictor}\" ({Damage} damage)";
	}
}

/// <summary>
/// Event dispatched on the host when something takes damage, so it can be modified.
/// </summary>
/// <param name="Damage">Information about the damage.</param>
public class ModifyDamageEvent
{
	public DamageInfo DamageInfo { get; set; }

	public ModifyDamageEvent( DamageInfo damageInfo )
	{
		DamageInfo = damageInfo;
	}

	/// <summary>
	/// Scales <see cref="ModifiedDamage"/> by <paramref name="damageScale"/>, adding the difference to <see cref="ArmorDamage"/>.
	/// </summary>
	public void ApplyArmor( float damageScale )
	{
		var reduced = DamageInfo.Damage * damageScale;

		DamageInfo = DamageInfo with { Damage = reduced, ArmorDamage = DamageInfo.Damage - reduced };
	}

	/// <summary>
	/// Adds a flag to this damage event.
	/// </summary>
	/// <param name="flag"></param>
	public void AddFlag( DamageFlags flag )
	{
		DamageInfo = DamageInfo with { Flags = DamageInfo.Flags | flag };
	}

	/// <summary>
	/// Removes a flag from this damage event.
	/// </summary>
	/// <param name="flag"></param>
	public void WithoutFlag( DamageFlags flag )
	{
		DamageInfo = DamageInfo with { Flags = DamageInfo.Flags & flag };
	}
}

/// <summary>
/// Event dispatched on a root object containing a <see cref="HealthComponent"/> that took damage.
/// </summary>
/// <param name="Damage">Information about the damage.</param>
public record DamageTakenEvent( DamageInfo DamageInfo );

/// <summary>
/// Event dispatched on a root object that inflicted damage on another object.
/// </summary>
/// <param name="Damage">Information about the damage.</param>
public record DamageGivenEvent( DamageInfo DamageInfo );

/// <summary>
/// Event dispatched in the scene when a <see cref="HealthComponent"/> died after taking damage.
/// </summary>
/// <param name="Damage">Information about the killing blow.</param>
public record KillEvent( DamageInfo DamageInfo );

public static class SceneTraceExtensions
{
	public static HitboxTags GetHitboxTags( this SceneTraceResult tr )
	{
		if ( tr.Hitbox is null ) return HitboxTags.None;

		var tags = HitboxTags.None;

		foreach ( var tag in tr.Hitbox.Tags )
		{
			if ( Enum.TryParse<HitboxTags>( tag, true, out var hitboxTag ) )
			{
				tags |= hitboxTag;
			}
		}

		return tags;
	}
}
