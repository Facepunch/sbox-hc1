using Sandbox.Events;

namespace Facepunch;

public sealed class TicketBasedScoring : Component,
	IGameEventHandler<KillEvent>
{
	[Property] public float DrainFrequency { get; set; } = 10;
	[Property] public int DrainAmountPerPoint { get; set; } = 1;

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

		GameMode.Instance.Get<TeamScoring>()?.IncrementScore( killerPlayer.Team, -1 );
	}

	public RealTimeUntil TimeUntilTick { get; set; } = 10;

	protected override void OnStart()
	{
		TimeUntilTick = DrainFrequency;
	}
}
