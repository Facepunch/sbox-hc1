using Sandbox.Events;

namespace Facepunch;

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
		bool foundGamemode = GameModes.Select( x => x.ToLowerInvariant() ).Contains( eventArgs.Title.ToLowerInvariant() );
		GameObject.Enabled = Inverse ? !foundGamemode : foundGamemode;
	}
}
