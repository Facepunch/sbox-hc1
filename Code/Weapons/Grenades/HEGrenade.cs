namespace Facepunch;

[Title( "HE Grenade" )]
public partial class HEGrenade : BaseGrenade, IMarkerObject
{
	[Property] public float DamageRadius { get; set; } = 512f;
	[Property] public float MaxDamage { get; set; } = 100f;
	[Property] public float ScreenShakeIntensity { get; set; } = 3f;
	[Property] public float ScreenShakeLifeTime { get; set; } = 1.5f;
	[Property] public Curve DamageFalloff { get; set; } = new Curve( new Curve.Frame( 1.0f, 1.0f ), new Curve.Frame( 0.0f, 0.0f ) );

	/// <summary>
	/// Temporary marker just to make it obvious there's a grenade
	/// </summary>
	MarkerFrame IMarkerObject.MarkerFrame => new MarkerFrame()
	{
		DisplayText = null,
		Position = Transform.Position,
		Rotation = Rotation.Identity,
		MaxDistance = 512f,
	};

	protected override void Explode()
	{
		if ( Networking.IsHost )
			Explosion.AtPoint( Transform.Position, DamageRadius, MaxDamage, ThrowerId, Id, DamageFalloff );

		var screenShaker = ScreenShaker.Main;
		var viewer = GameUtils.Viewer;
		
		if ( screenShaker.IsValid() && viewer.IsValid() )
		{
			var distance = viewer.Transform.Position.Distance( Transform.Position );
			var falloff = DamageFalloff;
			
			if ( falloff.Frames.Count == 0 )
			{
				falloff = new( new Curve.Frame( 1f, 1f ), new Curve.Frame( 0f, 0f ) );
			}

			var radiusWithPadding = DamageRadius * 1.2f;
			
			if ( distance <= radiusWithPadding )
			{
				var scalar = falloff.Evaluate( distance / radiusWithPadding );
				var shake = new ScreenShake.Random( ScreenShakeIntensity * scalar, ScreenShakeLifeTime );
				screenShaker.Add( shake );
			}
		}
		
		base.Explode();
	}

	/// <summary>
	/// Custom marker panel
	/// </summary>
	Type IMarkerObject.MarkerPanelTypeOverride => typeof( UI.GrenadeMarkerPanel );
}
