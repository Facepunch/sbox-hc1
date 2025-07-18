using Sandbox.Events;

namespace Facepunch;

[Title( "On Shot - Screen Shake" ), Category( "Weapon Components" ), Icon( "pending" )]
public class ScreenShakeOnShot : EquipmentComponent, IGameEventHandler<WeaponShotEvent>
{
	[Property] public float Length { get; set; } = 0.3f;
	[Property] public float Size { get; set; } = 1.05f;

	void IGameEventHandler<WeaponShotEvent>.OnGameEvent( WeaponShotEvent eventArgs )
	{
		if ( !Player.IsViewer ) return;

		var shake = new ScreenShake.Random( Length, Size );
		ScreenShaker.Main?.Add( shake );
	}
}
