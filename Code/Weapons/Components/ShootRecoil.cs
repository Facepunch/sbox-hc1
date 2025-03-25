namespace Facepunch;

[Title( "Recoil" ), Group( "Weapon Components" )]
public partial class ShootRecoil : EquipmentComponent
{
	[Property] public float ResetTime { get; set; } = 0.3f;

	// Recoil Patterns
	[Property, FeatureEnabled( "Recoil Pattern" )] public bool UseRecoilPattern { get; set; } = false;
	[Property, Category( "UseRecoilPattern" ), Feature( "Recoil Pattern" )] public Vector2 Scale { get; set; } = new Vector2( 2f, 5f );
	[Property, Category( "UseRecoilPattern" ), Feature( "Recoil Pattern" ) ] public RecoilPattern RecoilPattern { get; set; } = new();
	[Property, Group( "Standard Recoil" ), Feature( "Standard Recoil" )] public RangedFloat HorizontalSpread { get; set; } 
	[Property, Group( "Standard Recoil" ), Feature( "Standard Recoil" )] public RangedFloat VerticalSpread { get; set; }

	internal Angles Current { get; private set; }

	TimeSince TimeSinceLastShot;
	int currentFrame = 0;

	internal void Shoot()
	{
		if ( TimeSinceLastShot > ResetTime )
			currentFrame = 0;

		TimeSinceLastShot = 0;

		var timeDelta = Time.Delta;

		if ( UseRecoilPattern )
		{
			var point = RecoilPattern.GetPoint( ref currentFrame );

			var newAngles = new Angles( -point.y * Scale.y, -point.x * Scale.x, 0 ) * timeDelta;
			Current = Current + newAngles;
			currentFrame++;
		}
		else
		{
			var newAngles = new Angles( -VerticalSpread.GetValue() * timeDelta, HorizontalSpread.GetValue() * timeDelta, 0 );
			Current = Current + newAngles;
		}

	}

	protected override void OnUpdate()
	{
		if ( !Player.IsValid() )
			return;

		if ( !Player.IsLocallyControlled )
			return;

		Current = Current.LerpTo( Angles.Zero, Time.Delta * 10f );
	}
}
