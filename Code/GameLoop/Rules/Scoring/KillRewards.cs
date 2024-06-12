
using Facepunch;
using Sandbox.Events;

/// <summary>
/// Grants kill reward money.
/// </summary>
public sealed class KillRewards : Component, IGameEventHandler<KillEvent>
{
	[Property, HostSync]
	public bool AllowFriendlyFire { get; set; }

	[Property, HostSync, ShowIf( nameof(AllowFriendlyFire), false )]
	public int FriendlyFirePenalty { get; set; } = 300;

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		var damageInfo = eventArgs.DamageInfo;

		if ( GameUtils.GetPlayerFromComponent( damageInfo.Attacker ) is not { } killerPlayer )
			return;

		if ( GameUtils.GetPlayerFromComponent( damageInfo.Victim ) is not { } victimPlayer )
			return;

		if ( !AllowFriendlyFire && killerPlayer.IsFriendly( victimPlayer ) )
		{
			killerPlayer.Inventory.GiveCash( -FriendlyFirePenalty );
		}
		else if ( damageInfo.Inflictor is Weapon weapon )
		{
			killerPlayer.Inventory.GiveCash( weapon.Resource.KillReward );
		}
	}
}
