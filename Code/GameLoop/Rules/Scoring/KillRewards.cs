
using Facepunch;

/// <summary>
/// Grants kill reward money.
/// </summary>
public sealed class KillRewards : Component, IKillListener
{
	[Property, HostSync]
	public bool AllowFriendlyFire { get; set; }

	[Property, HostSync, ShowIf( nameof(AllowFriendlyFire), false )]
	public int FriendlyFirePenalty { get; set; } = 300;

	public void OnPlayerKilled( DamageEvent damageEvent )
	{
		if ( GameUtils.GetPlayerFromComponent( damageEvent.Attacker ) is not { } killerPlayer )
			return;

		if ( GameUtils.GetPlayerFromComponent( damageEvent.Victim ) is not { } victimPlayer )
			return;

		if ( !AllowFriendlyFire && killerPlayer.IsFriendly( victimPlayer ) )
		{
			killerPlayer.Inventory.GiveCash( -FriendlyFirePenalty );
		}
		else if ( damageEvent.Inflictor is Weapon weapon )
		{
			killerPlayer.Inventory.GiveCash( weapon.Resource.KillReward );
		}
	}
}
