
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
		Trace = true
	};

	bool IMarkerObject.ShouldShowMarker => true;

	protected override void Explode()
	{
		// Tell the server (as the owner of the grenade) to explode
		if ( ( Owner is null && Connection.Local.IsHost ) || Owner == GameUtils.LocalPlayer )
		{
			Log.Info( "Do explosion!" );
			Explosion.AtPoint( Transform.Position, DamageRadius, MaxDamage, Owner.Id, Id );
		}

		base.Explode();
	}
}
