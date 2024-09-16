using System.Text.Json.Serialization;
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Pick from a set of loadouts for each team.
/// </summary>
public sealed class RandomLoadoutAllocator : Component,
	IGameEventHandler<EnterStateEvent>
{
	public class LoadoutType : IWeighted
	{
		[KeyProperty]
		public string Title { get; set; } = "Unnamed";

		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public float Weight { get; set; } = 1f;

		/// <summary>
		/// If true, this loadout is only valid immediately after a team swap.
		/// </summary>
		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public bool OnlyAfterTeamSwap { get; set; }

		/// <summary>
		/// Minimum number of rounds between this loadout type.
		/// </summary>
		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public int MinRoundsSince { get; set; }

		public List<Loadout> Loadouts { get; set; } = new();

		/// <summary>
		/// Which round was this loadout type last used?
		/// </summary>
		[JsonIgnore, Hide]
		public int LastRound { get; set; }
	}

	public class Loadout : IWeighted
	{
		[KeyProperty]
		public Team Team { get; set; }

		[KeyProperty]
		public string Title { get; set; } = "Unnamed";

		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public float Weight { get; set; } = 1f;

		public List<EquipmentResource> Equipment { get; set; } = new();

		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public int Armor { get; set; } = 100;

		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public bool Helmet { get; set; } = true;

		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public bool DefuseKit { get; set; } = true;

		/// <summary>
		/// How many players must be on the team for this loadout to be valid.
		/// </summary>
		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public int MinTeamSize { get; set; } = 0;

		/// <summary>
		/// How many players can have this loadout.
		/// </summary>
		[JsonIgnore( Condition = JsonIgnoreCondition.Never )]
		public int MaxPlayers { get; set; } = 10;

		/// <summary>
		/// How many players currently have this loadout.
		/// </summary>
		[JsonIgnore, Hide]
		public int Players { get; set; }

		public void ApplyTo( PlayerPawn player )
		{
			++Players;

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
	/// Possible loadout types to give out.
	/// </summary>
	[Property]
	public List<LoadoutType> LoadoutTypes { get; set; } = new();

	[After<RespawnPlayers>, Before<SpecialWeaponAllocator>]
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		var roundCounter = GameMode.Instance.Get<RoundCounter>();
		var roundNumber = roundCounter?.Round ?? 1;
		var teamsSwapped = roundCounter?.RoundsSinceTeamSwap <= 1;

		Log.Info( $"round no: {roundNumber}, since swap: {roundCounter?.RoundsSinceTeamSwap}" );

		if ( SelectLoadoutType( roundNumber, teamsSwapped ) is not {} loadoutType )
		{
			return;
		}

		Log.Info( $"Loadout type: {loadoutType.Title}" );

		loadoutType.LastRound = roundNumber;

		foreach ( var teamPlayers in GameUtils.PlayerPawns.GroupBy( x => x.Team ) )
		{
			var teamSize = teamPlayers.Count();

			Log.Info( $"{teamPlayers.Key} team size: {teamSize}" );

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

				player.Inventory.SwitchToBest();

				if ( loadout.Players >= loadout.MaxPlayers )
				{
					teamLoadouts.Remove( loadout );
				}
			}
		}
	}

	private LoadoutType SelectLoadoutType( int roundNumber, bool teamsSwapped )
	{
		var types = LoadoutTypes
			.Where( x => x.LastRound <= 0 || x.MinRoundsSince <= roundNumber - x.LastRound )
			.Where( x => teamsSwapped || !x.OnlyAfterTeamSwap )
			.Where( x => x.Weight > 0f )
			.ToArray();

		if ( types.Length == 0 )
		{
			Log.Warning( $"No valid loadout types!" );
			return null;
		}

		return Random.Shared.FromListWeighted( types );
	}
}
