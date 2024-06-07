namespace Facepunch;

public sealed class DecoyGrenade : BaseGrenade
{
	[RequireComponent] Rigidbody Rigidbody { get; set; }

	[Property, Group( "Balance" )] public float ChatterSpeed { get; set; } = 50f;
	[Property, Group( "Balance" )] RangedFloat ShotDelay { get; set; } = new( 0.5f, 1.0f );
	[Property, Group( "Effects" )] SoundEvent ShotSound { get; set; }
	[Property, Group( "Effects" )] GameObject Effect { get; set; }

	bool CanChatter;
	TimeUntil TimeUntilShot = 0;

	protected override void OnUpdate()
	{
		CanChatter = Rigidbody.Velocity.Length < ChatterSpeed;

		if ( CanChatter )
		{
			if ( TimeUntilShot )
			{
				TimeUntilShot = ShotDelay.GetValue();
				Fire();
			}
		}
	}

	void Fire()
	{
		if ( ShotSound is not null)
			Sound.Play( ShotSound, Transform.Position );

		Effect?.Clone( new CloneConfig()
		{
			Transform = new Transform( Transform.Position ),
			StartEnabled = true
		} );
	}
}
