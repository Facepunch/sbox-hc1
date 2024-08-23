namespace Facepunch;

/// <summary>
/// Called to get local weapon stats for the player.
/// </summary>
internal static class WeaponStats
{
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
	/// <param name="amount"></param>
	internal static void Increment( string identifier, EquipmentResource resource, double amount = 1 )
	{
		var weaponId = resource.ResourceName;
		var statIdentifier = $"{identifier}-{weaponId}";
		Stats.Increment( statIdentifier, amount );
	}
}
