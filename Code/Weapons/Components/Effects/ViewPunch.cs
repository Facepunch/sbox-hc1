namespace Facepunch;

[Title( "On Shot - View Punch" ), Category( "Weapon Components" ), Icon( "pending" )]
public class ViewPunch : WeaponComponent, IShotListener
{
	[Property] public float Lifetime { get; set; } = 0.3f;
	[Property] public Vector3 PositionOffset { get; set; } = new( 0.5f, 0.2f, 0.75f );
	[Property] public Vector3 MaxPositionOffset { get; set; } = new( 0.5f, 0.2f, 0.75f );
	[Property] public Angles AnglesOffset { get; set; } = new( -0.5f, 0.1f, 0.5f );
	[Property] public Angles MaxAnglesOffset { get; set; } = new( -0.5f, 0.1f, 0.5f );
	[Property] public Curve Curve { get; set; }

	private Vector3 GetBetween( Vector3 one, Vector3 two )
	{
		var x = Game.Random.Float( one.x, two.x );
		var y = Game.Random.Float( one.y, two.y );
		var z = Game.Random.Float( two.z, two.z );
		return new Vector3( x, y, z );
	}

	private Angles GetBetween( Angles one, Angles two )
	{
		var pitch = Game.Random.Float( one.pitch, two.pitch );
		var yaw = Game.Random.Float( one.yaw, two.yaw );
		var roll = Game.Random.Float( two.roll, two.roll );
		return new Angles( pitch, yaw, roll );
	}

	void IShotListener.OnShot()
	{
		var shake = new ScreenShake.Punch( 
			Lifetime, 
			GetBetween( PositionOffset, MaxPositionOffset ), 
			GetBetween( AnglesOffset, MaxAnglesOffset ), 
			Curve );

		ScreenShaker.Main?.Add( shake );
	}
}
