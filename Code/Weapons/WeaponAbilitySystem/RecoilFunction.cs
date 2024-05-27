namespace Gunfight;

public partial class RecoilFunction : WeaponFunction
{
	[Property, Category( "Recoil" )] public float RecoilResetTime { get; set; } = 0.3f;
	[Property, Category( "Recoil" )] public RangedFloat HorizontalSpread { get; set; } = 0f;
	[Property, Category( "Recoil" )] public RangedFloat VerticalSpread { get; set; } = 0f;

	[Property, Category( "Scaling" )] public float PlayerVelocityLimit { get; set; } = 300f;
	[Property, Category( "Scaling" )] public float VelocitySpreadScale { get; set; } = 0.25f;

	internal Angles Current { get; private set; }

	TimeSince TimeSinceLastShot;
	int currentFrame = 0;

	private float HorizontalScale
	{
		get
		{
			var scale = 1f;
			// TODO: better accessor for stuff like this, this is mega shit
			var velLen = Weapon.PlayerController.CharacterController.Velocity.Length;
			scale += velLen.Remap( 0, PlayerVelocityLimit, 0, 1, true ) * VelocitySpreadScale;

			return scale;
		}
	}

	private float VerticalScale
	{
		get
		{
			var scale = 1f;
			// TODO: better accessor for stuff like this, this is mega shit
			var velLen = Weapon.PlayerController.CharacterController.Velocity.Length;
			scale += velLen.Remap( 0, PlayerVelocityLimit, 0, 1, true ) * VelocitySpreadScale;

			return scale;
		}
	}

	internal void Shoot()
	{
		if ( TimeSinceLastShot > RecoilResetTime )
		{
			currentFrame = 0;
		}

		TimeSinceLastShot = 0;

		var timeDelta = Time.Delta;
		var newAngles = new Angles( - (VerticalSpread.GetBetween() * VerticalScale ) * timeDelta, ( HorizontalSpread.GetBetween() * HorizontalScale ) * timeDelta, 0 );

		Current = Current + newAngles;
		currentFrame++;
	}

	protected override void OnUpdate()
	{
		Current = Current.LerpTo( Angles.Zero, Time.Delta * 10f );
	}
}

public static class Extensions
{
	public static float GetBetween( this RangedFloat self )
	{
		return Game.Random.Float( self.x, self.y );
	}
}
