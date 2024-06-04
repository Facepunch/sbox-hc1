using Sandbox;

namespace Facepunch.Editor;

public static class GameModeSystem
{
	/// <summary>
	/// We want to clear the gamemode when exiting play mode
	/// </summary>
	[Event( "scene.stop" )]
	public static void StopPlaying()
	{
		GameMode.ActivePath = null;
	}
}
