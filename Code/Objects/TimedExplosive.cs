
using Facepunch;

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

	/// <summary>
	/// Bomb site this bomb was planted at.
	/// </summary>
	public BombSite BombSite { get; private set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		TimeSincePlanted = 0f;
		TimeSinceLastBeep = 0f;

		BombSite = Zone.GetAt( Transform.Position )
			.Select( x => x.Components.Get<BombSite>() )
			.FirstOrDefault( x => x is not null );

		if ( BombSite is null )
		{
			Log.Warning( $"Bomb site is null!" );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		BeepEffects();

		if ( !Networking.IsHost ) return;

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

		foreach ( var listener in Scene.GetAllComponents<IBombDetonatedListener>() )
		{
			listener.OnBombDetonated( GameObject, BombSite );
		}

		if ( BombSite is not null )
		{
			// Bomb site determines damage, so safe radius can be tuned by the mapper

			foreach ( var health in Scene.GetAllComponents<HealthComponent>() )
			{
				var diff = health.Transform.Position - Transform.Position;
				var damage = BombSite.GetExplosionDamage( diff.Length );

				if ( damage <= 0f )
				{
					continue;
				}

				health.TakeDamage( damage, Transform.Position, diff.Normal * damage * 100f, Guid.Empty, Id );
			}
		}

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
