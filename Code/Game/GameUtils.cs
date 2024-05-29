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
	public static PlayerController LocalPlayer => Game.ActiveScene.GetSystem<PawnSystem>().Viewer as PlayerController;

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
	/// Get a player from a component that belongs to a player or their descendants.
	/// </summary>
	public static PlayerController GetPlayerFromComponent(Component component)
	{
		if ( component is PlayerController player ) return player;

		if ( component.GameObject.Root.Components.Get<PlayerController>( FindMode.EnabledInSelfAndDescendants ) is PlayerController foundPlayer )
		{
			return foundPlayer;
		}

		return null;
	}
}
