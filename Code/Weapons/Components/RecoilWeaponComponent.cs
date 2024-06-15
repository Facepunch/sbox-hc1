namespace Facepunch;

[Title( "Recoil" ), Group( "Weapon Components" )]
public partial class RecoilWeaponComponent : EquipmentComponent
{
	[Property, Category( "Recoil" )] public float ResetTime { get; set; } = 0.3f;

	// Recoil Patterns
	[Property, ToggleGroup( "UseRecoilPattern" )] public bool UseRecoilPattern { get; set; } = false;
	[Property, Category( "UseRecoilPattern" ), HideIf( "UseRecoilPattern", false )] public Vector2 Scale { get; set; } = new Vector2( 2f, 5f );
	[Property, Category( "UseRecoilPattern" ), HideIf( "UseRecoilPattern", false )] public RecoilPattern RecoilPattern { get; set; } = new();
	[Property, Group( "Standard Recoil" ), HideIf( "UseRecoilPattern", true )] public RangedFloat HorizontalSpread { get; set; } 
	[Property, Group( "Standard Recoil" ), HideIf( "UseRecoilPattern", true )] public RangedFloat VerticalSpread { get; set; }

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
		Current = Current.LerpTo( Angles.Zero, Time.Delta * 10f );
	}
}
