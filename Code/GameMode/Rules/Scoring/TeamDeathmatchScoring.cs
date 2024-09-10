using Facepunch;
using Sandbox.Events;

namespace Facepunch;

public sealed class TeamDeathmatchScoring : Component,
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

		if ( killerPlayer.Team is null )
			return;

		if ( victimPlayer.Team is null )
			return;

		GameMode.Instance.Get<TeamScoring>()?.IncrementScore( killerPlayer.Team );
	}
}
