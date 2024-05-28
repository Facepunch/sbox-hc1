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

	public static T GetHudPanel<T>()
		where T : Panel
	{
		var hudPanel = Game.ActiveScene?.GetAllComponents<MainHUD>()
			.FirstOrDefault();

		if ( hudPanel.IsValid() )
		{
			return hudPanel.Panel.Descendants.OfType<T>()
				.FirstOrDefault();
		}

		return null;
	}
}
