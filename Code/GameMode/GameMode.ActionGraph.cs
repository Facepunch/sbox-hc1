
namespace Facepunch;

partial class ActionGraphHelpers
{
	[ActionGraphNode( "gamemode" )]
	public static GameMode GetGameMode => GameMode.Instance;
}

