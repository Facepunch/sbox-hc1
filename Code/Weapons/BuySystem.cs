using Sandbox.Events;

namespace Facepunch;

public record class CanOpenBuyMenuEvent : IGameEvent
{
	public bool CanOpen { get; set; } = false;
}

public static class BuySystem
{
	public static bool IsEnabled()
	{
		if ( !GameUtils.LocalPlayer.IsValid() )
			return false;

		// Can't buy if dead
		if ( GameUtils.LocalPlayer.HealthComponent.State != LifeState.Alive )
			return false;

		var canOpenEvent = new CanOpenBuyMenuEvent();
		// Dispatch from the gamemode, not everything in the scene.
		GameMode.Instance.GameObject.Dispatch( canOpenEvent );
		return canOpenEvent.CanOpen;
	}

	public static bool IsInBuyZone()
	{
		var player = GameUtils.LocalPlayer;
		var zone = player.GetZone<BuyZone>();
		if ( zone is null )
			return false;

		if ( zone.Team == Team.Unassigned )
			return true;

		return zone.Team == player.Team;
	}
}
