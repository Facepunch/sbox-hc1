using System.Text.Json.Serialization;
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Pick from a set of loadouts for each team.
/// </summary>
public sealed class RandomLoadoutAllocator : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<RoundCounterIncrementedEvent>,
	IGameEventHandler<RoundCounterResetEvent>,
	IGameEventHandler<TeamsSwappedEvent>
{
	public class LoadoutType : IWeighted
	{
		[KeyProperty]
		public string Title { get; set; } = "Unnamed";
		public float Weight { get; set; } = 1f;

		/// <summary>
		/// If true, this loadout is only valid immediately after a team swap.
		/// </summary>
		public bool OnlyAfterTeamSwap { get; set; }

		/// <summary>
		/// Minimum number of rounds between this loadout type.
		/// </summary>
		public int MinRoundsSince { get; set; }

		public List<Loadout> Loadouts { get; set; } = new();

		/// <summary>
		/// How many rounds since this was last used?
		/// </summary>
		[JsonIgnore]
		public int RoundsSince { get; set; } = int.MaxValue;
	}

	public class Loadout : IWeighted
	{
		[KeyProperty]
		public Team Team { get; set; }
		[KeyProperty]
		public string Title { get; set; } = "Unnamed";
		public float Weight { get; set; } = 1f;
		public List<EquipmentResource> Equipment { get; set; } = new();
		public int Armor { get; set; } = 100;
		public bool Helmet { get; set; } = true;
		public bool DefuseKit { get; set; } = true;

		/// <summary>
		/// How many players must be on the team for this loadout to be valid.
		/// </summary>
		public int MinTeamSize { get; set; } = 0;

		/// <summary>
		/// How many players can have this loadout.
		/// </summary>
		public int MaxPlayers { get; set; } = 10;

		/// <summary>
		/// How many players currently have this loadout.
		/// </summary>
		public int Players { get; set; }

		public void ApplyTo( PlayerPawn player )
		{
			++Players;

			player.Inventory.Clear();

			player.Inventory.HasDefuseKit = DefuseKit && player.Team == Team.CounterTerrorist;

			player.ArmorComponent.HasHelmet = Helmet;
			player.ArmorComponent.Armor = Armor;

			foreach ( var equipment in Equipment )
			{
				player.Inventory.Give( equipment, false );
			}

			player.Inventory.RefillAmmo();
		}
	}

	/// <summary>
	/// Possible loadouts types to give out.
	/// </summary>
	[Property]
	public List<LoadoutType> LoadoutTypes { get; set; } = new();

	private bool _teamsSwapped;

	[After<RespawnPlayers>, After<DefaultEquipment>]
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( SelectLoadoutType() is not {} loadoutType )
		{
			return;
		}

		loadoutType.RoundsSince = 0;

		_teamsSwapped = false;

		foreach ( var teamPlayers in GameUtils.PlayerPawns.GroupBy( x => x.Team ) )
		{
			var teamSize = teamPlayers.Count();

			var teamLoadouts = loadoutType.Loadouts
				.Where( x => x.Team == teamPlayers.Key )
				.Where( x => x.MinTeamSize <= teamSize )
				.Where( x => x.Weight > 0f )
				.ToList();

			foreach ( var loadout in teamLoadouts )
			{
				loadout.Players = 0;
			}

			foreach ( var player in teamPlayers )
			{
				if ( teamLoadouts.Count == 0 )
				{
					Log.Warning( $"No valid loadouts for team {teamPlayers.Key}!" );
					break;
				}

				var loadout = Random.Shared.FromListWeighted( teamLoadouts );

				loadout.ApplyTo( player );

				if ( loadout.Players >= loadout.MaxPlayers )
				{
					teamLoadouts.Remove( loadout );
				}
			}
		}
	}

	private LoadoutType SelectLoadoutType()
	{
		var types = LoadoutTypes
			.Where( x => x.MinRoundsSince <= x.RoundsSince )
			.Where( x => _teamsSwapped || !x.OnlyAfterTeamSwap )
			.Where( x => x.Weight > 0f )
			.ToArray();

		if ( types.Length == 0 )
		{
			Log.Warning( $"No valid loadout types!" );
			return null;
		}

		return Random.Shared.FromListWeighted( types );
	}

	void IGameEventHandler<RoundCounterIncrementedEvent>.OnGameEvent( RoundCounterIncrementedEvent eventArgs )
	{
		foreach ( var loadoutType in LoadoutTypes )
		{
			loadoutType.RoundsSince += 1;
		}
	}

	void IGameEventHandler<RoundCounterResetEvent>.OnGameEvent( RoundCounterResetEvent eventArgs )
	{
		foreach ( var loadoutType in LoadoutTypes )
		{
			loadoutType.RoundsSince = int.MaxValue;
		}
	}

	void IGameEventHandler<TeamsSwappedEvent>.OnGameEvent( TeamsSwappedEvent eventArgs )
	{
		_teamsSwapped = true;
	}
}
