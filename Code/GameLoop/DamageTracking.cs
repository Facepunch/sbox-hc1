using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Track damage for every single player so we can call it back.
/// </summary>
public partial class DamageTracker : Component, IGameEventHandler<DamageTakenGlobalEvent>,
	IRoundCleanup,
	IPlayerEvents
{
	[Property] public bool ClearBetweenRounds { get; set; } = true;
	[Property] public bool ClearOnRespawn { get; set; } = false;

	public Dictionary<Client, List<DamageInfo>> Registry { get; set; } = new();
	public Dictionary<Client, List<DamageInfo>> MyInflictedDamage { get; set; } = new();

	[Rpc.Broadcast( NetFlags.HostOnly )]
	protected void RpcRefresh()
	{
		Refresh();
	}

	void IPlayerEvents.OnSpawned( PlayerPawn player )
	{
		if ( !ClearOnRespawn ) return;

		// Only include the owner
		using ( Rpc.FilterInclude( player.Network.Owner ) )
		{
			// Send the refresh
			RpcRefresh();
		}
	}

	public List<DamageInfo> GetDamageOnMe()
	{
		return GetDamageInflictedTo( Client.Local );
	}

	public List<DamageInfo> GetDamageInflictedTo( Client player )
	{
		if ( !Registry.TryGetValue( player, out var list ) )
		{
			return new List<DamageInfo>();
		}

		return list;
	}

	public List<DamageInfo> GetMyInflictedDamage( Client player )
	{
		if ( !MyInflictedDamage.TryGetValue( player, out var list ) )
		{
			return new List<DamageInfo>();
		}

		return list;
	}

	public struct GroupedDamage
	{
		public Client Attacker { get; set; }
		public int Count { get; set; }
		public float Damage { get; set; }
	}

	public List<GroupedDamage> GetGroupedDamage( Client player )
	{
		var groups = new List<GroupedDamage>();

		GetDamageInflictedTo( player )
			.GroupBy( x => x.Attacker )
			.ToList()
			.ForEach( group =>
			{
				groups.Add( new()
				{
					Attacker = group.First().Attacker is Pawn pawn ? pawn.Client : null,
					Count = group.Count(),
					Damage = group.Sum( x => x.Damage )
				} );
			} );


		return groups;
	}

	public List<GroupedDamage> GetGroupedInflictedDamage( Client player )
	{
		var groups = new List<GroupedDamage>();

		GetMyInflictedDamage( player )
			.GroupBy( x => x.Attacker )
			.ToList()
			.ForEach( group =>
			{
				groups.Add( new()
				{
					Attacker = group.First().Attacker is Pawn pawn ? pawn.Client : null,
					Count = group.Count(),
					Damage = group.Sum( x => x.Damage )
				} );
			} );


		return groups;
	}

	public void Refresh()
	{
		MyInflictedDamage.Clear();
		Registry.Clear();
	}

	void IGameEventHandler<DamageTakenGlobalEvent>.OnGameEvent( DamageTakenGlobalEvent eventArgs )
	{
		var attacker = eventArgs.DamageInfo.Attacker;
		var victim = eventArgs.DamageInfo.Victim;
		var Client = victim is Pawn pawn ? pawn.Client : null;

		if ( !Client.IsValid() )
			return;

		var attackerClient = attacker is Pawn attackerPawn ? attackerPawn.Client : null;
		if ( attackerClient == Client.Local )
		{
			if ( !MyInflictedDamage.TryGetValue( Client, out var myList ) )
			{
				MyInflictedDamage.Add( Client, new()
			{
				eventArgs.DamageInfo
			} );
			}
			else
			{
				myList.Add( eventArgs.DamageInfo );
			}
		}

		if ( !Registry.TryGetValue( Client, out var list ) )
		{
			Registry.Add( Client, new()
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
	void IRoundCleanup.OnRoundCleanup()
	{
		if ( !ClearBetweenRounds ) return;

		// This is called for everyone, so we don't need another RPC.

		// Get rid of old data since the rounds refreshed.
		Refresh();
	}
}
