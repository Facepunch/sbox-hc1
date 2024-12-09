using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Increments a stat when this state is entered.
/// </summary>
public sealed class StatIncrementer : Component, IGameEventHandler<EnterStateEvent>
{
	/// <summary>
	/// What's the stat?
	/// </summary>
	[Property]
	public string StatName { get; set; }

	/// <summary>
	/// Which team are we sending this to?
	/// </summary>
	[Property]
	public Team TeamFilter { get; set; }

	/// <summary>
	/// The stat value we'll set
	/// </summary>
	[Property]
	public int Amount { get; set; }

	/// <summary>
	/// Should we send an extra gamemdoe stat? Puts the gamemode ident after the stat name as a separate stat.
	/// </summary>
	[Property] 
	public bool SendExtraGameModeStat { get; set; }

	/// <summary>
	/// Since the state machine is sent on the host only, we have to RPC it
	/// </summary>
	[Rpc.Broadcast( NetFlags.HostOnly )]
	public void BroadcastStat()
	{
		Stats.Increment( StatName, Amount );

		if ( SendExtraGameModeStat )
		{
			Stats.Increment( StatName, Amount, true );
		}
	}

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( TeamFilter != Team.Unassigned )
		{
			using ( Rpc.FilterInclude( GameUtils.GetPlayers( TeamFilter ).Select( x => x.Connection ) ) )
			{
				BroadcastStat();
			}
		}
		else
		{
			BroadcastStat();
		}
	}

	[ConCmd( "hc1_stat_increment_test" )]
	public static void Dev_Increment()
	{
		if ( Game.IsEditor )
		{
			Stats.Increment( "wins", 1 );
			Stats.Increment( "wins", 1, true );
		}
	}
}
