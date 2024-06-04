using Facepunch.UI;

namespace Facepunch;

/// <summary>
/// A list of game utilities that'll help us achieve common goals with less code... I guess?
/// </summary>
public partial class GameUtils
{
	/// <summary>
	/// The locally-controlled <see cref="PlayerController"/>, if there is one.
	/// </summary>
	public static PlayerController LocalPlayer = null;

	/// <summary>
	/// The <see cref="PlayerController"/> we're in the perspective of if there is one.
	/// </summary>
	public static PlayerController Viewer => Game.ActiveScene.GetSystem<PawnSystem>().Viewer as PlayerController;

	/// <summary>
	/// All players, both assigned to a team and spectating.
	/// </summary>
	public static IEnumerable<PlayerController> AllPlayers => Game.ActiveScene.GetAllComponents<PlayerController>();

	/// <summary>
	/// Players assigned to a team, so not spectating.
	/// </summary>
	public static IEnumerable<PlayerController> ActivePlayers => AllPlayers.Where( x => x.TeamComponent.Team != Team.Unassigned );

	/// <summary>
	/// Players not assigned to a team, or spectating.
	/// </summary>
	public static IEnumerable<PlayerController> InactivePlayers => AllPlayers.Where( x => x.TeamComponent.Team == Team.Unassigned );

	/// <summary>
	/// Players assigned to a particular team.
	/// </summary>
	public static IEnumerable<PlayerController> GetPlayers( Team team ) => AllPlayers.Where( x => x.TeamComponent.Team == team );

	/// <summary>
	/// Get all spawn point transforms for the given team.
	/// </summary>
	public static IEnumerable<Transform> GetSpawnPoints( Team team ) => Game.ActiveScene
		.GetAllComponents<TeamSpawnPoint>()
		.Where( x => x.Team == team )
		.Select( x => x.Transform.World )
		.Concat( Game.ActiveScene.GetAllComponents<SpawnPoint>()
			.Select( x => x.Transform.World ) );

	/// <summary>
	/// Pick a random spawn point for the given team.
	/// </summary>
	public static Transform GetRandomSpawnPoint( Team team )
	{
		return Random.Shared.FromArray( GetSpawnPoints( team ).ToArray(), Transform.Zero );
	}

	/// <summary>
	/// Helper list of the two teams.
	/// </summary>
	public static IReadOnlyList<Team> Teams { get; } = new[] { Team.Terrorist, Team.CounterTerrorist };

	/// <summary>
	/// Get a player from a component that belongs to a player or their descendants.
	/// </summary>
	public static PlayerController GetPlayerFromComponent( Component component )
	{
		if ( component is PlayerController player ) return player;
		if ( !component.IsValid() ) return null;
		return !component.GameObject.IsValid() ? null : component.GameObject.Root.Components.Get<PlayerController>( FindMode.EnabledInSelfAndDescendants );
	}

	public static Weapon GetWeaponFromComponent( Component inflictor )
	{
		if ( inflictor is Weapon weapon )
		{
			return weapon;
		}

		return null;
	}

	public static void GiveTeamIncome( Team team, int amount )
	{
		foreach ( var player in GetPlayers( team ) )
		{
			player.Inventory.GiveCash( amount );
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
