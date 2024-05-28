
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
	}

	private void BeepEffects()
	{
		var t = Math.Clamp( TimeSincePlanted / Duration, 0f, 1f );

		if (TimeSinceLastBeep > BeepFrequency.Evaluate( t ))
		{
			TimeSinceLastBeep = 0f;

			if (BeepSound is not null)
			{
				Sound.Play( BeepSound, Transform.Position );
			}
		}
	}
}
