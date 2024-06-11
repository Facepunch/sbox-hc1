namespace Facepunch;

public class FovOffset : WeaponComponent, IShotListener
{
	[Property] public float Length { get; set; } = 0.3f;
	[Property] public float Size { get; set; } = 1.05f;
	[Property] public Curve Curve { get; set; }

	void IShotListener.OnShot()
	{
		var shake = new ScreenShake.Fov( Length, Size, Curve );
		ScreenShaker.Main?.Add( shake );
	}
}
