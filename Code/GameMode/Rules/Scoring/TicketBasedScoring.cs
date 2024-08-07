using Sandbox.Events;

namespace Facepunch;

public sealed class TicketBasedScoring : Component,
	IGameEventHandler<KillEvent>
{
	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		if ( !Networking.IsHost )
			return;

		var damageInfo = eventArgs.DamageInfo;

		if ( GameUtils.GetPlayerFromComponent( damageInfo.Attacker ) is not { } killerPlayer )
			return;

		if ( GameUtils.GetPlayerFromComponent( damageInfo.Victim ) is not { } victimPlayer )
			return;

		if ( killerPlayer.IsFriendly( victimPlayer ) )
			return;

		if ( killerPlayer.Team == Team.Unassigned )
			return;

		if ( victimPlayer.Team == Team.Unassigned )
			return;

		GameMode.Instance.Get<TeamScoring>()?.IncrementScore( killerPlayer.Team, -1 );
	}

	// TODO: capture event from capture point
	// TODO: tick event that looks at all capture points and drains tickets
}
