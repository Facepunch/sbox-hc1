
public sealed class TimedExplosive : Component
{
	[Property, Category( "Config" )]
	public float Duration { get; set; } = 45f;

	[Sync]
	public TimeSince TimeSincePlanted { get; private set; }

	public TimeSince TimeSinceLastBeep { get; private set; }

	[Property, Category( "Effects" )]
	public SoundEvent BeepSound { get; set; }

	[Property, Category( "Effects" )]
	public Curve BeepFrequency { get; set; } = new Curve( new Curve.Frame( 0f, 1f ), new Curve.Frame( 1f, 0.25f ) );

	[Property, Category( "Effects" )]
	public GameObject ExplosionPrefab { get; set; }

	[Property, Category( "Effects" )]
	public GameObject BeepEffectPrefab { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		TimeSincePlanted = 0f;
		TimeSinceLastBeep = 0f;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		BeepEffects();

		if ( IsProxy ) return;

		if ( TimeSincePlanted > Duration )
		{
			Enabled = false;
			Explode();
		}
	}

	private void Explode()
	{
		if ( ExplosionPrefab is null ) return;

		var explosion = ExplosionPrefab.Clone( Transform.Position, Rotation.FromYaw( Random.Shared.NextSingle() * 360f ) );

		explosion.NetworkSpawn();

		GameObject.Destroy();
	}

	private void BeepEffects()
	{
		var t = Math.Clamp( TimeSincePlanted / Duration, 0f, 1f );

		if ( TimeSinceLastBeep > BeepFrequency.Evaluate( t ) )
		{
			TimeSinceLastBeep = 0f;

			if ( BeepSound is not null )
			{
				Sound.Play( BeepSound, Transform.Position );
			}

			if ( BeepEffectPrefab is not null )
			{
				BeepEffectPrefab.Clone( Transform.Position + Vector3.Up * 4 );
			}
		}
	}
}
