namespace Facepunch;

public static class DefuseExtensions
{
	/// <summary>
	/// Helper to check if a player is planting, useful for UI.
	/// </summary>
	public static bool IsPlanting( this PlayerPawn player, out DefuseC4 DefuseC4 )
	{
		if ( !player.CurrentEquipment.IsValid() )
		{
			DefuseC4 = null;
			return false;
		}

		DefuseC4 = player.CurrentEquipment?.GetComponentInChildren<DefuseC4>();
		return DefuseC4 is { Active: true, IsPlanting: true };
	}

	/// <summary>
	/// Helper to check if a player is defusing, useful for UI.
	/// </summary>
	public static bool IsDefusing( this PlayerPawn player, out TimedExplosive bomb )
	{
		if ( player.LastUsedObject?.GetComponentInChildren<TimedExplosive>() is { DefusingPlayer: { } defuser } match && defuser == player )
		{
			bomb = match;
			return true;
		}

		bomb = null;
		return false;
	}
}
