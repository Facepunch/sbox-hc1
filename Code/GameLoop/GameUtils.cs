using System.IO;

namespace Facepunch;

public interface IWeighted
{
	float Weight { get; }
}

/// <summary>
/// A list of game utilities that'll help us achieve common goals with less code... I guess?
/// </summary>
public static partial class GameUtils
{
	/// <summary>
	/// All players in the game (includes disconnected players before expiration).
	/// </summary>
	public static IEnumerable<PlayerState> AllPlayers => Game.ActiveScene.GetAllComponents<PlayerState>();

	/// <summary>
	/// Get all players on a team.
	/// </summary>
	public static IEnumerable<PlayerState> GetPlayers( Team team ) => AllPlayers.Where( x => x.Team == team );

	/// <summary>
	/// Every <see cref="PlayerPawn"/> currently in the world.
	/// </summary>
	public static IEnumerable<PlayerPawn> PlayerPawns => AllPlayers.Select( x => x.PlayerPawn ).Where( x => x.IsValid() );

	/// <summary>
	/// Every <see cref="PlayerPawn"/> currently in the world, on the given team.
	/// </summary>
	public static IEnumerable<PlayerPawn> GetPlayerPawns( Team team ) => PlayerPawns.Where( x => x.Team == team );

	public static IDescription GetDescription( GameObject go ) => go?.GetComponent<IDescription>();
	public static IDescription GetDescription( Component component ) => GetDescription( component?.GameObject );

	public static IEnumerable<SceneFile> GetAvailableMaps()
	{
		return ResourceLibrary.GetAll<SceneFile>().Where( x => x.GetMetadata( "IsVisibleInMenu", null ).ToBool() is true );
	}

	/// <summary>
	/// Get all spawn point transforms for the given team.
	/// </summary>
	public static IEnumerable<SpawnPointInfo> GetSpawnPoints( Team team, params string[] tags ) => Game.ActiveScene
		.GetAllComponents<TeamSpawnPoint>()
		.Where( x => x.Team == team )
		.Where( x => tags.Length == 0 || tags.Any( x.Tags.Contains ) )
		.Select( x => new SpawnPointInfo( x.Transform.World, x.GameObject.Tags.ToArray() ) )
		.Concat( Game.ActiveScene.GetAllComponents<SpawnPoint>()
			.Select( x => new SpawnPointInfo( x.Transform.World, x.GameObject.Tags.ToArray() ) ) );

	/// <summary>
	/// Get all spawn point transforms
	/// </summary>
	public static IEnumerable<SpawnPointInfo> GetSpawnPoints( params string[] tags ) => Game.ActiveScene
		.GetAllComponents<TeamSpawnPoint>()
		.Where( x => tags.Length == 0 || tags.Any( x.Tags.Contains ) )
		.Select( x => new SpawnPointInfo( x.Transform.World, x.GameObject.Tags.ToArray() ) )
		.Concat( Game.ActiveScene.GetAllComponents<SpawnPoint>()
			.Select( x => new SpawnPointInfo( x.Transform.World, x.GameObject.Tags.ToArray() ) ) );

	/// <summary>
	/// Pick a random spawn point for the given team.
	/// </summary>
	public static SpawnPointInfo GetRandomSpawnPoint( Team team, params string[] tags )
	{
		return Random.Shared.FromArray( GetSpawnPoints( team, tags ).ToArray(),
			new SpawnPointInfo( Transform.Zero, Array.Empty<string>() ) );
	}

	/// <summary>
	/// Helper list of the two teams.
	/// </summary>
	public static IReadOnlyList<Team> Teams { get; } = new[] { Team.Terrorist, Team.CounterTerrorist };

	/// <summary>
	/// Get a player from a component that belongs to a player or their descendants.
	/// </summary>
	public static PlayerPawn GetPlayerFromComponent( Component component )
	{
		if ( component is PlayerPawn player ) return player;
		if ( !component.IsValid() ) return null;
		return !component.GameObject.IsValid() ? null : component.GameObject.Root.GetComponentInChildren<PlayerPawn>();
	}

	/// <summary>
	/// Get a player from a component that belongs to a player or their descendants.
	/// </summary>
	public static Pawn GetPawn( Component component )
	{
		if ( component is Pawn pawn ) return pawn;
		if ( !component.IsValid() ) return null;
		return !component.GameObject.IsValid() ? null : component.GameObject.Root.GetComponentInChildren<Pawn>();
	}

	public static Equipment FindEquipment( Component inflictor )
	{
		if ( inflictor is Equipment equipment )
		{
			return equipment;
		}

		return null;
	}

	/// <summary>
	/// Returns the invoking client to the main menu
	/// </summary>
	public static void ReturnToMainMenu()
	{
		var sc = ResourceLibrary.Get<SceneFile>( "scenes/menu.scene" );
		Game.ActiveScene.Load( sc );
	}

	/// <summary>
	/// Log all known player states to the console.
	/// </summary>
	public static void LogPlayers()
	{
		/*
		var writer = new StringWriter();

		writer.WriteLine( "All players:" );

		foreach ( var player in AllPlayers )
		{
			writer.WriteLine( $"  {player.GameObject.Name}:" );
			writer.WriteLine( $"    Id: {player.Id}" );
			writer.WriteLine( $"    DisplayName: {player.DisplayName}" );
			writer.WriteLine( $"    IsConnected: {player.IsConnected}" );
			writer.WriteLine( $"    IsLocalPlayer: {player.IsLocalPlayer}" );
			writer.WriteLine( $"    Connection: {(player.Connection is { } connection ? $"{connection.Id} ({connection.DisplayName})" : "null")}" );
			writer.WriteLine( $"    Team: {player.Team}" );
			writer.WriteLine( $"    UniqueId: {player.PlayerId?.UniqueId}" );
			writer.WriteLine( $"    TeamUniqueId: {player.PlayerId?.TeamUniqueId}" );
			writer.WriteLine( $"    PlayerPawn: {player.PlayerPawn?.Id.ToString() ?? "null"}" );
		}

		writer.WriteLine();
		writer.WriteLine( "All pawns:" );

		foreach ( var pawn in Game.ActiveScene.GetAllComponents<PlayerPawn>() )
		{
			writer.WriteLine( $"  {pawn.GameObject.Name}:" );
			writer.WriteLine( $"    Id: {pawn.Id}" );
			writer.WriteLine( $"    DisplayName: {pawn.DisplayName}" );
			writer.WriteLine( $"    PlayerState: {pawn.PlayerState?.Id.ToString() ?? "null"}" );
		}

		writer.WriteLine();
		writer.WriteLine( "All connections:" );

		foreach ( var connection in Connection.All )
		{
			writer.WriteLine( $"  {connection.Name}:" );
			writer.WriteLine( $"    Id: {connection.Id}" );
			writer.WriteLine( $"    DisplayName: {connection.DisplayName}" );
			writer.WriteLine( $"    PartyId: {connection.PartyId}" );
			writer.WriteLine( $"    SteamId: {connection.SteamId}" );
			writer.WriteLine( $"    IsConnecting: {connection.IsConnecting}" );
			writer.WriteLine( $"    IsActive: {connection.IsActive}" );
		}

		Log.Info( writer.ToString() );
		*/
	}

	public static T FromListWeighted<T>( this Random random, IReadOnlyList<T> list, T defaultValue = default )
		where T : IWeighted
	{
		if ( list.Count == 0 )
		{
			return defaultValue;
		}

		var totalWeight = list.Sum( x => x.Weight );

		if ( totalWeight <= 0f )
		{
			return defaultValue;
		}

		var value = random.NextSingle() * totalWeight;

		foreach ( var item in list )
		{
			if ( item.Weight < 0f )
			{
				throw new ArgumentException( "Weights must all be >= 0." );
			}

			value -= item.Weight;

			if ( value <= 0f )
			{
				return item;
			}
		}

		throw new Exception( "We should have returned an item already!" );
	}
}
