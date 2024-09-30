using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// If added to a gamemode, we'll record weapon stats while in this state.
/// </summary>
public sealed class WeaponStatsRule : Component,
	IGameEventHandler<KillEvent>
{
	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		var player = GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Attacker );
		if ( !player.IsValid() )
			return;

		var inflictor = eventArgs.DamageInfo.Inflictor;
		if ( inflictor is Equipment wpn && wpn.IsValid() )
		{
			using ( Rpc.FilterInclude( player.Network.Owner ) )
			{
				SendKillStat( wpn.Resource.ResourcePath, eventArgs.DamageInfo.Hitbox, eventArgs.DamageInfo.Flags );
			}
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	private void SendKillStat( string resourcePath, HitboxTags hitbox = default, DamageFlags flags = default )
	{
		var resource = ResourceLibrary.Get<EquipmentResource>( resourcePath );
		if ( resource is not null )
		{
			if ( hitbox == HitboxTags.Head )
			{
				WeaponStats.Increment( "kills-headshots", resource, flags );
			}

			WeaponStats.Increment( "kills", resource, flags );
		}
	}
}
