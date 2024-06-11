namespace Facepunch;

[Title( "HE Grenade" )]
public partial class HEGrenade : BaseGrenade, IMarkerObject
{
	[Property] public float DamageRadius { get; set; } = 512f;
	[Property] public float MaxDamage { get; set; } = 100f;


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
		
		base.Explode();
	}

	/// <summary>
	/// Custom marker panel
	/// </summary>
	Type IMarkerObject.MarkerPanelTypeOverride => typeof( UI.GrenadeMarkerPanel );
}
