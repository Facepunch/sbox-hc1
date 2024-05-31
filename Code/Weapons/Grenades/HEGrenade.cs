namespace Facepunch;

[Title( "HE Grenade" )]
public partial class HEGrenade : BaseGrenade, IMarkerObject
{
	[Property] public float DamageRadius { get; set; } = 512f;
	[Property] public float MaxDamage { get; set; } = 100f;

	/// <summary>
	/// Temporary marker just to make it obvious there's a grenade
	/// </summary>
	MarkerFrame IMarkerObject.MarkerFrame => new MarkerFrame()
	{
		DisplayText = "GRENADE!",
		Position = Transform.Position,
		Rotation = Transform.Rotation,
		MaxDistance = 512f,
	};

	bool IMarkerObject.ShouldShowMarker => true;

	protected override void Explode()
	{
		if ( Networking.IsHost )
		{
			Explosion.AtPoint( Transform.Position, DamageRadius, MaxDamage, ThrowerId, Id );
		}
		
		base.Explode();
	}
}
