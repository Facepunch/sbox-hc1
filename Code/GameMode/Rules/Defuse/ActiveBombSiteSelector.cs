using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Selects one bomb site to be active at the start of this state, enabling spawn points with that
/// site's name as a tag.
/// </summary>
public sealed class ActiveBombSiteSelector : Component,
	IGameEventHandler<EnterStateEvent>
{
	[HostSync]
	public Guid CurrentBombSiteId { get; set; }

	public BombSite CurrentBombSite => Scene.Directory.FindComponentByGuid( CurrentBombSiteId ) as BombSite;

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

		CurrentBombSiteId = bombSite.Id;

		var name = bombSite.GameObject.Name.ToLower();
		var spawns = Scene.Components.GetAll<TeamSpawnPoint>( FindMode.EverythingInSelfAndDescendants );

		foreach ( var spawn in spawns )
		{
			spawn.Enabled = spawn.GameObject.Tags.Contains( name );
		}
	}
}
