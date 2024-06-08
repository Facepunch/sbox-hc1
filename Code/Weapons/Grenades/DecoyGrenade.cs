namespace Facepunch;

public sealed class DecoyGrenade : BaseGrenade, ICustomMinimapIcon
{
	[RequireComponent] Rigidbody Rigidbody { get; set; }

	[Property, Group( "Balance" )] public float ChatterSpeed { get; set; } = 50f;
	[Property, Group( "Balance" )] RangedFloat ShotDelay { get; set; } = new( 0.5f, 1.0f );
	[Property, Group( "Effects" )] SoundEvent ShotSound { get; set; }
	[Property, Group( "Effects" )] GameObject Effect { get; set; }

	bool CanChatter;
	TimeUntil TimeUntilShot = 0;
	TimeSince TimeSinceShot = 1;

	MinimapIconType IMinimapIcon.IconType => MinimapIconType.PlayerEnemy;
	Vector3 IMinimapElement.WorldPosition => Transform.Position;
	string ICustomMinimapIcon.CustomStyle => "background-image-tint: rgba(255, 0, 0, 1 );";
	bool IMinimapElement.IsVisible( PlayerController viewer ) => TimeSinceShot < 0.5f && IsEnemy;

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

		base.OnUpdate();
	}

	void Fire()
	{
		TimeSinceShot = 0;

		if ( ShotSound is not null )
			Sound.Play( ShotSound, Transform.Position );

		Effect?.Clone( new CloneConfig()
		{
			Parent = GameObject,
			Transform = new Transform(),
			StartEnabled = true
		} );
	}
}
