using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Selects one bomb site to be active at the start of this state, enabling spawn points with that
/// site's name as a tag.
/// </summary>
public sealed class ActiveBombSiteSelector : Component,
	IGameEventHandler<EnterStateEvent>
{
	[Sync( SyncFlags.FromHost )] public BombSite CurrentBombSite { get; set; }

	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		var bombSites = Scene.Components
			.GetAll<BombSite>( FindMode.EverythingInSelfAndDescendants )
			.ToArray();

		if ( bombSites.Length == 0 )
		{
			throw new Exception( "No valid bomb sites!" );
		}

		var bombSite = Random.Shared.FromArray( bombSites );

		CurrentBombSite = bombSite;

		var name = bombSite.GameObject.Name.ToLower();
		var spawns = Scene.GetComponentsInChildren<TeamSpawnPoint>();

		foreach ( var spawn in spawns )
		{
			spawn.Enabled = spawn.GameObject.Tags.Contains( name );
		}

		GameMode.Instance.ShowToast( $"Retakes: {bombSite.Zone.DisplayName}", duration: 3f );
	}
}
