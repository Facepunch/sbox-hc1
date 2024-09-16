using Sandbox.Events;

namespace Facepunch;

public sealed class TicketBasedScoring : Component,
	IGameEventHandler<KillEvent>,
	IGameEventHandler<CapturePointCapturedEvent>
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

		if ( killerPlayer.Team == Team.Unassigned )
			return;

		if ( victimPlayer.Team == Team.Unassigned )
			return;

		GameMode.Instance.Get<TeamScoring>()?.IncrementScore( killerPlayer.Team, -1 );
	}

	public RealTimeUntil TimeUntilTick { get; set; } = 10;

	protected override void OnStart()
	{
		TimeUntilTick = DrainFrequency;
	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( !TimeUntilTick )
			return;

		foreach ( var capturePoint in Scene.GetAllComponents<CapturePoint>() )
		{
			if ( capturePoint.Team != Team.Unassigned )
			{
				// Hurt the opponents tickets
				GameMode.Instance.Get<TeamScoring>()?.IncrementScore( capturePoint.Team.GetOpponents(), -1 );
			}
		}

		TimeUntilTick = DrainFrequency;
	}

	void IGameEventHandler<CapturePointCapturedEvent>.OnGameEvent( CapturePointCapturedEvent eventArgs )
	{
		// TODO: Scores from capturing? Maybe?
	}
}
