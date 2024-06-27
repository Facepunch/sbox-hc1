namespace Facepunch;

public static class DefuseExtensions
{
	/// <summary>
	/// Helper to check if a player is planting, useful for UI.
	/// </summary>
	public static bool IsPlanting( this PlayerPawn player, out BombPlantComponent BombPlantComponent )
	{
		if ( !player.CurrentEquipment.IsValid() )
		{
			BombPlantComponent = null;
			return false;
		}

		BombPlantComponent = player.CurrentEquipment?.Components?.Get<BombPlantComponent>( FindMode.EnabledInSelfAndDescendants );
		return BombPlantComponent is { Active: true, IsPlanting: true };
	}

	/// <summary>
	/// Helper to check if a player is defusing, useful for UI.
	/// </summary>
	public static bool IsDefusing( this PlayerPawn player, out TimedExplosive bomb )
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
