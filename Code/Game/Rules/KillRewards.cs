
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

	public void OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force, Component inflictor = null, bool isHeadshot = false )
	{
		if ( GameUtils.GetPlayerFromComponent( killer ) is not { } killerPlayer )
		{
			return;
		}

		if ( GameUtils.GetPlayerFromComponent( victim ) is not { } victimPlayer )
		{
			return;
		}

		if ( !AllowFriendlyFire && killerPlayer.IsFriendly( victimPlayer ) )
		{
			killerPlayer.Inventory.GiveCash( -FriendlyFirePenalty );
		}
		else if ( GameUtils.GetWeaponFromComponent( inflictor ) is {} weapon )
		{
			killerPlayer.Inventory.GiveCash( weapon.Resource.KillReward );
		}
	}
}
