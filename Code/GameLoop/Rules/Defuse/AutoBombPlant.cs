using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Forces whoever is holding the C4 to immediately plant it.
/// </summary>
public sealed class AutoBombPlant : Component,
	IGameEventHandler<EnterStateEvent>
{
	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		var bombCarrier = GameUtils.PlayerPawns
			.FirstOrDefault( x => x.Inventory.HasBomb );

		var bomb = bombCarrier?.Inventory.Equipment
			.Select( x => x.GetComponentInChildren<DefuseC4>() )
			.FirstOrDefault( x => x != null );

		if ( bomb is null )
		{
			Log.Warning( "No bomb carrier!" );
			return;
		}

		bomb.FinishPlant( bombCarrier.SpawnPosition, true );
	}
}
