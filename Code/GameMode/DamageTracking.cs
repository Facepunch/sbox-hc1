using Sandbox.Events;

namespace Facepunch;

public partial class DamageTracker : Component, IGameEventHandler<DamageTakenGlobalEvent>,
	IGameEventHandler<BetweenRoundCleanupEvent>,
	IGameEventHandler<PlayerSpawnedEvent>
{
	[Property] public bool ClearBetweenRounds { get; set; } = true;
	[Property] public bool ClearOnRespawn { get; set; } = false;

	[Property] public Dictionary<PlayerState, List<Facepunch.DamageInfo>> Registry { get; set; } = new();

	[Broadcast( NetPermission.HostOnly )]
	protected void RpcRefresh()
	{
		Refresh();
	}

	public List<Facepunch.DamageInfo> GetDamageOnMe()
	{
		return GetDamageInflictedTo( PlayerState.Local );
	}

	public List<Facepunch.DamageInfo> GetDamageInflictedTo( PlayerState player )
	{
		if ( !Registry.TryGetValue( player, out var list ) )
		{
			return new List<Facepunch.DamageInfo>();
		}

		return list;
	}

	public struct GroupedDamage
	{
		public PlayerState Attacker { get; set; }
		public int Count { get; set; }
		public float Damage { get; set; }
	}

	public List<GroupedDamage> GetGroupedDamage( PlayerState player )
	{
		var groups = new List<GroupedDamage>();

		GetDamageInflictedTo( player )
			.GroupBy( x => x.Attacker )
			.ToList()
			.ForEach( group =>
		{
			groups.Add( new()
			{
				Attacker = group.First().Attacker is Pawn pawn ? pawn.PlayerState : null,
				Count = group.Count(),
				Damage = group.Sum( x => x.Damage )
			} );
		} );


		return groups;
	}

	public void Refresh()
	{
		Registry.Clear();
	}

	void IGameEventHandler<DamageTakenGlobalEvent>.OnGameEvent( DamageTakenGlobalEvent eventArgs )
	{
		var victim = eventArgs.DamageInfo.Victim;
		var playerState = victim is Pawn pawn ? pawn.PlayerState : null;

		if ( !playerState.IsValid() )
			return;

		if ( !Registry.TryGetValue( playerState, out var list ) )
		{
			Registry.Add( playerState, new()
			{
				eventArgs.DamageInfo
			} );
		}
		else
		{
			list.Add( eventArgs.DamageInfo );
		}
	}

	/// <summary>
	/// Called between rounds.
	/// </summary>
	/// <param name="eventArgs"></param>
	void IGameEventHandler<BetweenRoundCleanupEvent>.OnGameEvent( BetweenRoundCleanupEvent eventArgs )
	{
		if ( !ClearBetweenRounds ) return;
		
		// This is called for everyone, so we don't need another RPC.

		// Get rid of old data since the rounds refreshed.
		Refresh();
	}

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		if ( !ClearOnRespawn ) return;

		// Only include the owner
		using ( Rpc.FilterInclude( eventArgs.Player.Network.OwnerConnection ) )
		{
			// Send the refresh
			RpcRefresh();
		}
	}
}
