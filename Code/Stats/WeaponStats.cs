namespace Facepunch;

/// <summary>
/// Called to get local weapon stats for the player.
/// </summary>
internal static class WeaponStats
{
	public static IEnumerable<string> GetStatsFrom( DamageFlags flags )
	{
		var idents = new List<string>();
		if ( flags.HasFlag( DamageFlags.WallBang ) ) idents.Add( "wallbang" );
		if ( flags.HasFlag( DamageFlags.AirShot ) ) idents.Add( "airshot" );

		return idents;
	}

	/// <summary>
	/// Get a weapon stat
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="resource"></param>
	/// <returns></returns>
	internal static double Get( string identifier, EquipmentResource resource )
	{
		var weaponId = resource.ResourceName;
		var statIdentifier = $"{identifier}-{weaponId}";
		return Stats.Get( statIdentifier, 0 );
	}

	/// <summary>
	/// Increment a weapon stat
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="resource"></param>
	/// <param name="damageFlags"></param>
	/// <param name="amount"></param>
	internal static void Increment( string identifier, EquipmentResource resource, DamageFlags damageFlags = DamageFlags.None, double amount = 1 )
	{
		var weaponId = resource.ResourceName;
		var statIdentifier = $"{identifier}-{weaponId}";
		Stats.Increment( statIdentifier, amount );

		foreach ( var extraIdent in GetStatsFrom( damageFlags ) )
		{
			// Global stat
			statIdentifier = $"{identifier}-{extraIdent}";
			Stats.Increment( statIdentifier, amount );

			// Weapon specific stat
			statIdentifier += $"-{weaponId}";
			Stats.Increment( statIdentifier, amount );
		}
	}
}
