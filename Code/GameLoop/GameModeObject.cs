using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Keep this GameObject alive if we match the specific gamemode criteria - so it only spawns when playing specific modes.
/// </summary>
public partial class GameModeObject : Component, IGameEventHandler<GamemodeInitializedEvent>
{
	/// <summary>
	/// What gamemodes are we gonna work for?
	/// </summary>
	[Property] public List<string> GameModes { get; set; }

	/// <summary>
	/// Should we inverse the logic? Only disable if we select these gamemodes?
	/// </summary>
	[Property] public bool Inverse { get; set; } = false;

	void IGameEventHandler<GamemodeInitializedEvent>.OnGameEvent( GamemodeInitializedEvent eventArgs )
	{
		if ( string.IsNullOrEmpty( eventArgs.Title ) )
		{
			// Dunno how this can happen, but it is what it is 
			return;
		}

		if ( !GameObject.IsValid() )
		{
			// Dunno how this can happen, but it is what it is 
			return;
		}

		bool foundGamemode = GameModes.Select( x => x.ToLowerInvariant() ).Contains( eventArgs.Title.ToLowerInvariant() );
		GameObject.Enabled = Inverse ? !foundGamemode : foundGamemode;
	}
}
