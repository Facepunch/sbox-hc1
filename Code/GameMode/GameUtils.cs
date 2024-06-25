using Facepunch.UI;

namespace Facepunch;

/// <summary>
/// A list of game utilities that'll help us achieve common goals with less code... I guess?
/// </summary>
public partial class GameUtils
{
	/// <summary>
	/// The locally-controlled <see cref="PlayerPawn"/>, if there is one.
	/// </summary>
	public static PlayerPawn LocalPlayer => LocalPlayerState.CurrentPlayerPawn;

	/// <summary>
	/// The <see cref="PlayerState"/> we're in the perspective of.
	/// </summary>
	public static PlayerState Viewer => PlayerState.CurrentPlayerState;

	/// <summary>
	/// The <see cref="Pawn"/> we're in the perspective of if there is one.
	/// </summary>
	public static Pawn CurrentPawn => LocalPlayerState.Pawn;

	/// <summary>
	/// Our local player state.
	/// </summary>
	public static PlayerState LocalPlayerState = null;

	// TODO: use states everywhere?
	public static IEnumerable<PlayerState> AllPlayerStates => Game.ActiveScene.GetAllComponents<PlayerState>();
	public static IEnumerable<PlayerState> ActivePlayerStates => AllPlayerStates.Where( x => x.Team != Team.Unassigned );
	public static IEnumerable<PlayerState> InactivePlayerStates => GetPlayerStates( Team.Unassigned );
	public static IEnumerable<PlayerState> GetPlayerStates( Team team ) => AllPlayerStates.Where( x => x.Team == team );

	/// <summary>
	/// All players, both assigned to a team and spectating.
	/// </summary>
	public static IEnumerable<PlayerPawn> AllPlayers => Game.ActiveScene.GetAllComponents<PlayerPawn>();

	/// <summary>
	/// Players assigned to a team, so not spectating.
	/// </summary>
	public static IEnumerable<PlayerPawn> ActivePlayers => AllPlayers.Where( x => x.Team != Team.Unassigned );

	/// <summary>
	/// Players not assigned to a team, or spectating.
	/// </summary>
	public static IEnumerable<PlayerPawn> InactivePlayers => AllPlayers.Where( x => x.Team == Team.Unassigned );

	/// <summary>
	/// Players assigned to a particular team.
	/// </summary>
	public static IEnumerable<PlayerPawn> GetPlayers( Team team ) => AllPlayers.Where( x => x.Team == team );

	/// <summary>
	/// Get all spawn point transforms for the given team.
	/// </summary>
	public static IEnumerable<Transform> GetSpawnPoints( Team team, params string[] tags ) => Game.ActiveScene
		.GetAllComponents<TeamSpawnPoint>()
		.Where( x => x.Team == team )
		.Where( x => tags.Length == 0 || tags.Any( x.Tags.Contains )  )
		.Select( x => x.Transform.World )
		.Concat( Game.ActiveScene.GetAllComponents<SpawnPoint>()
			.Select( x => x.Transform.World ) );

	/// <summary>
	/// Pick a random spawn point for the given team.
	/// </summary>
	public static Transform GetRandomSpawnPoint( Team team, params string[] tags )
	{
		return Random.Shared.FromArray( GetSpawnPoints( team, tags ).ToArray(), Transform.Zero );
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
		return !component.GameObject.IsValid() ? null : component.GameObject.Root.Components.Get<PlayerPawn>( FindMode.EnabledInSelfAndDescendants );
	}

	/// <summary>
	/// Get a player from a component that belongs to a player or their descendants.
	/// </summary>
	public static Pawn GetPawn( Component component )
	{
		if ( component is Pawn pawn ) return pawn;
		if ( !component.IsValid() ) return null;
		return !component.GameObject.IsValid() ? null : component.GameObject.Root.Components.Get<Pawn>( FindMode.EnabledInSelfAndDescendants );
	}

	/// <summary>
	/// Get a player from a component that belongs to a player or their descendants.
	/// </summary>
	public static PlayerState GetPlayerState( Component component )
	{
		var pawn = GetPawn( component );
		if ( !pawn.IsValid() )
			return null;

		return pawn.PlayerState;
	}

	public static Equipment FindEquipment( Component inflictor )
	{
		if ( inflictor is Equipment equipment )
		{
			return equipment;
		}

		return null;
	}

	public static void GiveTeamIncome( Team team, int amount )
	{
		foreach ( var player in GetPlayers( team ) )
		{
			player.PlayerState.GiveCash( amount );
		}
	}

	/// <summary>
	/// Returns the invoking client to the main menu
	/// </summary>
	public static void ReturnToMainMenu()
	{
		var sc = ResourceLibrary.Get<SceneFile>( "scenes/menu.scene" );
		Game.ActiveScene.Load( sc );
	}
}
