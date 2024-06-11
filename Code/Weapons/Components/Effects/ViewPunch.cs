namespace Facepunch;

public class ViewPunch : WeaponComponent, IShotListener
{
	[Property] public float Lifetime { get; set; } = 0.3f;
	[Property] public Vector3 PositionOffset { get; set; } = new( 0.5f, 0.2f, 0.75f );
	[Property] public Angles AnglesOffset { get; set; } = new( -0.5f, 0.1f, 0.5f );
	[Property] public Curve Curve { get; set; }

	void IShotListener.OnShot()
	{
		var shake = new ScreenShake.Punch( Lifetime, PositionOffset, AnglesOffset, Curve );
		ScreenShaker.Main?.Add( shake );
	}
}
