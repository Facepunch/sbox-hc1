namespace Facepunch;

/// <summary>
/// An ActionGraph helper so we can access the gamemode in an ActionGraph.
/// </summary>
partial class ActionGraphHelpers
{
	[ActionGraphNode( "gamemode" )]
	public static GameMode GetGameMode => GameMode.Instance;
}

