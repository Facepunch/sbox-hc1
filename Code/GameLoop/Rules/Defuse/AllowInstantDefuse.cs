using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Instantly defuse the bomb if all opponents are dead, and no grenades are nearby.
/// </summary>
public sealed class AllowInstantDefuse : Component,
	IGameEventHandler<BombDefuseStartEvent>
{
	public void OnGameEvent( BombDefuseStartEvent eventArgs )
	{
		var explosive = eventArgs.Bomb?.GetComponent<TimedExplosive>();

		if ( explosive is null ) return;
		if ( AnyTerroristsAlive ) return;
		if ( !HasEnoughTime( explosive ) ) return;
		if ( AnyNearbyGrenades( eventArgs.Defuser ) ) return;

		explosive.FinishDefusing();
	}

	private bool AnyTerroristsAlive => GameUtils.GetPlayerPawns( Team.Terrorist )
		.Any( x => x.HealthComponent.State == LifeState.Alive );

	private bool HasEnoughTime( TimedExplosive explosive )
	{
		if ( explosive is null ) return false;

		var untilExplode = explosive.Duration - explosive.TimeSincePlanted;

		// If it's going to be close, let them suffer

		return explosive.DefuseTime < untilExplode - 1;
	}

	private bool AnyNearbyGrenades( PlayerPawn player )
	{
		if ( Scene.GetAllComponents<BaseGrenade>().Any( x => x.CanDealDamage ) ) return true;

		var maxDistSq = 128f * 128f;

		if ( MolotovFireNode.All.Any( x => (x.Transform.Position - player.Transform.Position).LengthSquared < maxDistSq ) )
		{
			return true;
		}

		return false;
	}
}

