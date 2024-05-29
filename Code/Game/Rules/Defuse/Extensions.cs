using Facepunch;

public static class DefuseExtensions
{
	/// <summary>
	/// Helper to check if a player is planting, useful for UI.
	/// </summary>
	public static bool IsPlanting( this PlayerController player, out PlantFunction plantFunction )
	{
		plantFunction = player.Inventory.CurrentWeapon?.GetFunction<PlantFunction>();
		return plantFunction is { Active: true, IsPlanting: true };
	}

	/// <summary>
	/// Helper to check if a player is defusing, useful for UI.
	/// </summary>
	public static bool IsDefusing( this PlayerController player, out TimedExplosive bomb )
	{
		if ( player.LastUsedObject?.Components.Get<TimedExplosive>() is { DefusingPlayer: { } defuser } match && defuser == player )
		{
			bomb = match;
			return true;
		}

		bomb = null;
		return false;
	}
}
