namespace Gunfight;

public partial class DamageInfo
{
	/// <summary>
	/// Creates a generic DamageInfo struct.
	/// </summary>
	/// <param name="damage"></param>
	/// <returns></returns>
	public static DamageInfo Generic( float damage )
	{
		return new DamageInfo
		{
			Damage = damage
		};
	}

	/// <summary>
	/// Creates a DamageInfo struct assuming it's bullet damage.
	/// </summary>
	/// <param name="damage"></param>
	/// <returns></returns>
	public static DamageInfo Bullet( float damage, GameObject attacker, GameObject inflictor = null )
	{
		if ( !inflictor.IsValid() ) inflictor = attacker;

		return new DamageInfo
		{
			Damage = damage,
			Attacker = attacker,
			Inflictor = inflictor,
			Tags = new[] { "bullet" }
		};
	}
}
