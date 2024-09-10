using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Increments a stat when this state is entered, to the highest scoring team.
/// </summary>
public sealed class StatIncrementerHighestTeam : Component, IGameEventHandler<EnterStateEvent>
{
	/// <summary>
	/// What's the stat?
	/// </summary>
	[Property]
	public string StatName { get; set; }


	/// <summary>
	/// The stat value we'll set
	/// </summary>
	[Property]
	public int Amount { get; set; } = 1;

	/// <summary>
	/// Should we send an extra gamemdoe stat? Puts the gamemode ident after the stat name as a separate stat.
	/// </summary>
	[Property] 
	public bool SendExtraGameModeStat { get; set; }

	private TeamScoring TeamScoring => GameMode.Instance.Get<TeamScoring>( true );

	/// <summary>
	/// Since the state machine is sent on the host only, we have to RPC it
	/// </summary>
	[Broadcast( NetPermission.HostOnly )]
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
		var winner = TeamScoring.GetHighest();
		
		if ( winner is null ) 
			return;

		using ( Rpc.FilterInclude( GameUtils.GetPlayers( winner ).Select( x => x.Connection ) ) )
		{
			BroadcastStat();
		}
	}
}
