using Facepunch.UI;
using Sandbox.Diagnostics;

namespace Facepunch;

[Hide]
public partial class MapVoteSystem : SingletonComponent<MapVoteSystem>
{
	/// <summary>
	/// Our networked list of voting options.
	/// </summary>
	[Sync] private NetList<NetworkedOption> Options { get; set; } = new();

	/// <summary>
	/// Our networked list of votes.
	/// </summary>
	[Sync] public NetList<NetworkedVote> Votes { get; set; } = new();

	/// <summary>
	/// How long until we decide who won?
	/// </summary>
	public RealTimeUntil TimeUntilDecidedWinner { get; private set; } = 0;

	/// <summary>
	/// How long until we move server?
	/// </summary>
	public RealTimeUntil TimeUntilTransfer { get; private set; } = 0;

	/// <summary>
	/// Which option is the winner?
	/// </summary>
	public Option? WinningOption { get; private set; }

	public IEnumerable<Option> VoteOptions => FromNetworked( Options );

	/// <summary>
	/// An instance of the map vote overlay UI
	/// </summary>
	[RequireComponent] MapVoteOverlay MapVoteOverlay { get; set; }

	/// <summary>
	/// The screen panel which draws our UI
	/// </summary>
	[RequireComponent] ScreenPanel ScreenPanel { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		TimeUntilDecidedWinner = 15;
		ScreenPanel.ZIndex = 999;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( TimeUntilDecidedWinner )
		{
			TimeUntilTransfer = 5;
			WinningOption = GetWinningOption();

			if ( !WinningOption.HasValue )
			{
				// TODO: we found no winner, pick random from next best value
				GameUtils.ReturnToMainMenu();
			}
		}

		if ( WinningOption is not null && TimeUntilTransfer )
		{
			Game.ActiveScene.Load( WinningOption.Value.Map.SceneFile );
		}
	}

	/// <summary>
	/// Converts a list of networked options to readable ones
	/// </summary>
	/// <param name="networked"></param>
	/// <returns></returns>
	private static IEnumerable<Option> FromNetworked( IEnumerable<NetworkedOption> networked )
	{
		return networked.Select( x => x.AsNormal() );
	}

	/// <summary>
	/// Converts a list of normal options to networked ones
	/// </summary>
	/// <param name="normal"></param>
	/// <returns></returns>
	private static IEnumerable<NetworkedOption> ToNetworked( IEnumerable<Option> normal )
	{
		return normal.Select( x => x.AsNetworked() );
	}

	private Option GetOption( int index )
	{
		return Options[ index ].AsNormal();
	}

	/// <summary>
	/// What's our current winning option?
	/// TODO: provide sorted options instead so other systems can take winner, or take a draw 
	/// </summary>
	/// <returns></returns>
	public Option? GetWinningOption()
	{
		var dict = new Dictionary<Option, int>();

		foreach ( var vote in Votes )
		{
			var option = GetOption( vote.Option );

			if ( !dict.TryGetValue( option, out var _ ) )
			{
				dict.Add( option, 0 );
			}

			dict[option]++;
		}

		var highestKeyValuePair = dict.OrderByDescending( x => x.Value )
			.ToDictionary( pair => pair.Key, pair => pair.Value )
			.FirstOrDefault();

		if ( highestKeyValuePair.Value < 1 ) 
			return null;

		return highestKeyValuePair.Key;
	}

	public static IEnumerable<Option> GenerateOptions( GameModeInfo currentMode = null, MapDefinition currentMap = null, int count = 5 )
	{
		var maps = ResourceLibrary.GetAll<MapDefinition>().ToList();
		var options = new List<Option>();

		var inList = ( GameModeInfo gameMode ) =>
		{
			return options.FirstOrDefault( x => x.Mode == gameMode ).Mode is not null;
		};

		int attempts = 0;
		while ( options.Count < count )
		{
			// Grab a random map
			var map = Game.Random.FromList( maps );

			// Grab a random mode that is supported by this map, and is not one we picked before
			var modes = GameMode.GetAll( map.SceneFile );
			var mode = Game.Random.FromList( modes.Where( x => !inList( x ) ).ToList() );

			// We ran out of options :S
			if ( mode is null )
			{
				maps.Remove( map );
				attempts++;

				continue;
			}

			if ( attempts > 5 )
				break;

			options.Add( new Option()
			{
				Map = map,
				Mode = mode
			} );
		}

		return options;
	}

	/// <summary>
	/// Start a vote. This'll make a component on the server and 
	/// </summary>
	/// <param name="currentMode"></param>
	/// <param name="currentMap"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	public static MapVoteSystem Create( GameModeInfo currentMode = null, MapDefinition currentMap = null, int count = 5 )
	{
		// Host-only
		Assert.True( Networking.IsHost );

		if ( Instance.IsValid() )
		{
			Instance.GameObject.Destroy();
		}

		var generated = GenerateOptions( currentMode, currentMap, count )
			.ToList();

		// Push the active scene
		using var _ = Game.ActiveScene.Push();

		var go = new GameObject();
		var system = go.Components.Create<MapVoteSystem>();

		// Add our generated options to the NetList so it's sent to clients
		foreach ( var option in ToNetworked( generated ) )
		{
			system.Options.Add( option );
		}

		// Spawn this object over the network
		go.NetworkSpawn();

		return system;
	}

	/// <summary>
	/// TODO: remove me
	/// </summary>
	[ConCmd( "hc1_dev_mapvote" )]
	public static void Dev_CreateMapVote()
	{
		Create();
	}

	/// <summary>
	/// RPCs a vote, targeted to the host, so it's not exposed to any public API.
	/// </summary>
	/// <param name="index"></param>
	[Broadcast]
	private void RpcVote( int index )
	{
		var callerSteamId = Rpc.Caller.SteamId;

		// did we already get a winning option?
		if ( WinningOption is { } option )
			return;

		Log.Info( $"{callerSteamId} trying to vote for index {index}" );

		var existingVote = Votes.FirstOrDefault( x => x is not null && x.VoterId == callerSteamId );
		if ( existingVote is not null )
		{
			Votes.Remove( existingVote );
		}

		// Don't re-add if we vote for the same thing
		if ( existingVote is not null && existingVote.Option == index ) return;

		Votes.Add( new NetworkedVote()
		{
			 Option = index,
			 VoterId = callerSteamId,
		} );
	}

	/// <summary>
	/// Sends a vote to the host.
	/// </summary>
	/// <param name="index"></param>
	public void Vote( int index )
	{
		using ( Rpc.FilterInclude( Connection.Host ) )
		{
			RpcVote( index );
		}
	}

	/// <summary>
	/// A networked vote.
	/// </summary>
	public class NetworkedVote
	{
		/// <summary>
		/// The player's SteamId
		/// </summary>
		public ulong VoterId { get; set; }

		/// <summary>
		/// Which option are we voting for? (Mapped to the list)
		/// </summary>
		public int Option { get; set; }
	}

	/// <summary>
	/// A voting option that has real data.
	/// </summary>
	public record struct Option
	{
		/// <summary>
		/// What mode are we playing?
		/// </summary>
		public GameModeInfo Mode { get; set; }

		/// <summary>
		/// What map?
		/// </summary>
		public MapDefinition Map { get; set; }

		/// <summary>
		/// Converts this to a networkable <see cref="NetworkedOption"/>
		/// </summary>
		/// <returns></returns>
		public NetworkedOption AsNetworked()
		{
			return new NetworkedOption
			{
				ModeResource = Mode.Path,
				MapResource = Map.ResourcePath
			};
		}

		public override string ToString()
		{
			return $"{Mode}, {Map}";
		}
	}

	/// <summary>
	/// A networkable variant of <see cref="Option"/>, since it only passes through strings.
	/// </summary>
	public struct NetworkedOption
	{
		/// <summary>
		/// The full resource path of the map.
		/// </summary>
		public string MapResource { get; set; }

		/// <summary>
		/// The full resource path of the gamemode.
		/// </summary>
		public string ModeResource { get; set; }

		/// <summary>
		/// Convert this to a readable <see cref="Option"/>
		/// </summary>
		/// <returns></returns>
		public Option AsNormal()
		{
			var mapDef = ResourceLibrary.Get<MapDefinition>( MapResource );
			var modePath = ModeResource;
			var mode = GameMode.GetAll( mapDef.SceneFile )
				.FirstOrDefault( x => x.Path == modePath );

			return new Option
			{
				Map = mapDef,
				Mode = mode
			};
		}

		public override string ToString()
		{
			return $"{MapResource}, {ModeResource}";
		}
	}
}
