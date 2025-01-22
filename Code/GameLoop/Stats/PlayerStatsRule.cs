using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// If added to a gamemode, we'll record player stats while in this state.
/// </summary>
public sealed class PlayerStatsRule : Component,
	IGameEventHandler<KillEvent>
{
	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		SendToAttacker( eventArgs );
		SendToVictim( eventArgs );
	}

	private void SendToAttacker( KillEvent eventArgs )
	{
		var player = GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Attacker );
		if ( !player.IsValid() )
			return;

		// Bots don't care about stats
		if ( player.Client.IsBot )
			return;

		// TODO: Don't count team-kills?

		var inflictor = eventArgs.DamageInfo.Inflictor;
		if ( inflictor is Equipment wpn && wpn.IsValid() )
		{
			using ( Rpc.FilterInclude( player.Network.Owner ) )
			{
				SendKillStat( eventArgs.DamageInfo.Hitbox );
			}
		}
	}

	/// <summary>
	/// Send a death stat to the victim. This can only happen if the killer was a player.
	/// </summary>
	/// <param name="eventArgs"></param>
	private void SendToVictim( KillEvent eventArgs )
	{
		var player = GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Victim );
		if ( !player.IsValid() )
			return;

		// Player left?
		if ( !player.Client.IsValid() )
			return;

		// Bots don't care about stats
		if ( player.Client.IsBot )
			return;

		var attacker = GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Attacker );
		if ( !attacker.IsValid() )
			return;

		// TODO: Don't count team-kills?

		using ( Rpc.FilterInclude( player.Network.Owner ) )
		{
			SendDeathStat();
		}
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void SendKillStat( HitboxTags hitbox = default )
	{
		if ( hitbox == HitboxTags.Head )
		{
			Stats.Increment( "kills-headshots" );
		}

		Stats.Increment( "kills" );
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void SendDeathStat()
	{
		Stats.Increment( "deaths" );
	}
}
