using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// A weapon component. This can be anything that controls a weapon. Aiming, recoil, sway, shooting..
/// </summary>
[Icon( "track_changes" )]
public partial class OnKillEquipmentComponent : EquipmentComponent, IGameEventHandler<KillEvent>
{
	[Property] public GameObject PrefabToSpawn { get; set; }

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		if ( GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Victim ) is { } player 
			&& player == Equipment.Owner )
		{
			PrefabToSpawn.Clone( Equipment.WorldPosition );
		}
	}
}
