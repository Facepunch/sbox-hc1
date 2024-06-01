namespace Facepunch;

[Icon( "track_changes" )]
public partial class ShootWeaponComponent : InputWeaponComponent
{
	[Property, Group( "Bullet" )] public float BaseDamage { get; set; } = 25.0f;
	[Property, Group( "Bullet" )] public float FireRate { get; set; } = 0.2f;
	[Property, Group( "Bullet" )] public float DryFireDelay { get; set; } = 1f;
	[Property, Group( "Bullet" )] public float BulletSize { get; set; } = 1.0f;
	[Property, Group( "Bullet" )] public int BulletCount { get; set; } = 1;

	[Property, Group( "Bullet Falloff" )] public Curve BaseDamageFalloff { get; set; } = new( new List<Curve.Frame>() { new( 0, 1 ), new( 1, 0 ) } );
	[Property, Group( "Bullet Falloff" )] public float MaxRange { get; set; } = 1024000;

	[Property, Group( "Bullet Spread" )] public float BulletSpread { get; set; } = 0;
	[Property, Group( "Bullet Spread" )] public float PlayerVelocityLimit { get; set; } = 300f;
	[Property, Group( "Bullet Spread" )] public float VelocitySpreadScale { get; set; } = 0.25f;
	[Property, Group( "Bullet Spread" )] public float InAirSpreadMultiplier { get; set; } = 2f;

	/// <summary>
	/// What sound should we play when we fire?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent ShootSound { get; set; }

	/// <summary>
	/// What sound should we play when we dry fire?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent DryFireSound { get; set; }

	/// <summary>
	/// The current weapon's ammo container.
	/// </summary>
	[Property, Category( "Ammo" )] public AmmoComponent AmmoComponent { get; set; }

	/// <summary>
	/// Does this weapon require an ammo container to fire its bullets?
	/// </summary>
	[Property, Category( "Ammo" )] public bool RequiresAmmoComponent { get; set; } = false;

	/// <summary>
	/// How many ricochet hits until we stop traversing
	/// </summary>
	[Property, Group( "Ricochet" )] protected float RicochetMaxHits { get; set; } = 2f;

	/// <summary>
	/// Maximum angle in degrees for ricochet to be possible
	/// </summary>
	[Property, Group( "Ricochet" )] public float MaxRicochetAngle { get; set; } = 45f;

	/// <summary>
	/// Anything past 2048 units won't produce effects,
	/// This is squared.
	/// </summary>
	[Property, Group( "Effects" )] public float MaxEffectsPlayDistance { get; set; } = 4194304f;

	/// <summary>
	/// Accessor for the aim ray.
	/// </summary>
	protected Ray WeaponRay => Weapon.PlayerController.AimRay;

	/// <summary>
	/// How long since we shot?
	/// </summary>
	public TimeSince TimeSinceShoot { get; private set; }

	/// <summary>
	/// Fetches the desired model renderer that we'll focus effects on like trail effects, muzzle flashes, etc.
	/// </summary>
	protected SkinnedModelRenderer EffectsRenderer
	{
		get
		{
			if ( IsProxy || !Weapon.ViewModel.IsValid() )
				return Weapon.ModelRenderer;

			return Weapon.ViewModel.ModelRenderer;
		}
	}

	/// <summary>
	/// Store a reference to the blood impact sound so we don't have to grab it every time.
	/// </summary>
	private static SoundEvent BloodImpactSound;

	protected override void OnStart()
	{
		if ( BloodImpactSound is not null ) return;
		BloodImpactSound = ResourceLibrary.Get<SoundEvent>( "sounds/impacts/bullets/impact-bullet-flesh.sound" );
	}

	/// <summary>
	/// Play any particle effects such as muzzle flashes.
	/// </summary>
	[Broadcast]
	protected void DoShootEffects()
	{
		if ( !EffectsRenderer.IsValid() )
			return;

		if ( ShootSound is not null )
		{
			if ( Sound.Play( ShootSound, Weapon.Transform.Position ) is { } snd )
			{
				snd.ListenLocal = Weapon.PlayerController?.IsViewer ?? false;
				Log.Trace( $"ShootWeaponComponent: ShootSound {ShootSound.ResourceName}" );
			}
		}

		// Third person
		Weapon.PlayerController?.BodyRenderer.Set( "b_attack", true );

		// First person
		Weapon.ViewModel?.ModelRenderer.Set( "b_attack", true );
	}

	private LegacyParticleSystem CreateParticleSystem( string particle, Vector3 pos, Rotation rot, float decay = 5f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Transform.Position = pos;
		gameObject.Transform.Rotation = rot;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = ParticleSystem.Load( particle );
		gameObject.Transform.ClearInterpolation();

		// Clear off in a suitable amount of time.
		gameObject.DestroyAsync( decay );

		return p;
	}

	[Broadcast]
	private void CreateBloodEffects( Vector3 pos, Vector3 normal )
	{
		if ( !IsNearby( pos ) )
			return;

		if ( BloodImpactSound is null )
			return;

		var particlePath = "particles/impact.flesh.bloodpuff.vpcf";
		CreateParticleSystem( particlePath, pos, Rotation.LookAt( -normal ), 0.5f );

		var snd = Sound.Play( BloodImpactSound, pos );
		snd.ListenLocal = Weapon?.PlayerController?.IsViewer ?? false;
	}

	/// <summary>
	/// Shoot the gun!
	/// </summary>
	public void Shoot()
	{
		TimeSinceShoot = 0;

		if ( AmmoComponent is not null )
		{
			AmmoComponent.Ammo--;
		}

		// If we have a recoil function, let it know.
		Weapon.Components.Get<RecoilWeaponComponent>( FindMode.EnabledInSelfAndDescendants )?.Shoot();

		DoShootEffects();

		for ( int i = 0; i < BulletCount; i++ )
		{
			int count = 0;

			foreach ( var tr in GetShootTrace() )
			{
				if ( !tr.Hit )
					continue;

				// CreateImpactEffects( tr.Surface, tr.EndPosition, tr.Normal );
				DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance, count );

				if ( tr.GameObject?.Root.Components.Get<PlayerController>( FindMode.EnabledInSelfAndDescendants ) is { } player )
				{
					CreateBloodEffects( tr.HitPosition, tr.Normal );
				}

				var damage = CalculateDamageFalloff( BaseDamage, tr.Distance );

				Log.Info( $"base damage: {BaseDamage}, real damage: {damage}" );

				// Inflict damage on whatever we find.
				tr.GameObject.TakeDamage( damage, tr.EndPosition, tr.Direction * tr.Distance, Weapon.PlayerController.HealthComponent.Id, Weapon.Id, tr.Hitbox?.Tags?.Has( "head" ) ?? false );
				count++;
			}
		}
	}

	private float CalculateDamageFalloff( float damage, float distance )
	{
		var distDelta = distance / MaxRange;
		var damageMultiplier = BaseDamageFalloff.Evaluate( distDelta );

		return damage * damageMultiplier;
	}

	/// <summary>
	/// Are we nearby a position? Used for FX
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	private bool IsNearby( Vector3 position )
	{
		return position.DistanceSquared( Scene.Camera.Transform.Position ) < MaxEffectsPlayDistance;
	}

	/// <summary>
	/// Makes some tracers using legacy particle effects.
	/// </summary>
	[Broadcast]
	protected void DoTracer( Vector3 startPosition, Vector3 endPosition, float distance, int count )
	{
		if ( !IsNearby( startPosition ) || !IsNearby( endPosition ) ) return;

		var effectPath = "particles/gameplay/guns/trail/trail_smoke.vpcf";
		if ( count > 0 ) effectPath = "particles/gameplay/guns/trail/rico_trail_smoke.vpcf";

		var origin = count == 0 ? EffectsRenderer.GetAttachment( "muzzle" )?.Position ?? startPosition : startPosition;
		var ps = CreateParticleSystem( effectPath, origin, Rotation.Identity, 1f );
		ps.SceneObject.SetControlPoint( 0, origin );
		ps.SceneObject.SetControlPoint( 1, endPosition );
		ps.SceneObject.SetControlPoint( 2, distance );
	}

	protected void DryShoot()
	{
		TimeSinceShoot = 0f;
		DryShootEffects();
	}

	[Broadcast]
	protected void DryShootEffects()
	{
		if ( DryFireSound is not null )
		{
			var snd = Sound.Play( DryFireSound, Weapon.Transform.Position );
			snd.ListenLocal = !IsProxy;
			Log.Trace( $"ShootWeaponComponent: ShootSound {DryFireSound.ResourceName}" );
		}

		// First person
		Weapon.ViewModel?.ModelRenderer.Set( "b_attack_dry", true );
	}

	protected SceneTraceResult DoTraceBullet( Vector3 start, Vector3 end, float radius )
	{
		return Scene.Trace.Ray( start, end )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "ragdoll" )
			.Size( radius )
			.Run();
	}

	protected virtual Vector3 CalculateRicochetDirection( SceneTraceResult tr, ref float hits )
	{
		if ( tr.GameObject?.Tags.Has( "glass" ) ?? false )
		{
			// Allow us to do another hit
			hits--;
			return tr.Direction;
		}

		return Vector3.Reflect( tr.Direction, tr.Normal ).Normal;
	}

	/// <summary>
	/// Gets the current bullet spread which is accumulated by the player's movement speed, or if they're in the air.
	/// </summary>
	/// <returns></returns>
	float GetBulletSpread()
	{
		var spread = BulletSpread;
		var velLen = Weapon.PlayerController.CharacterController.Velocity.Length;
		spread += velLen.Remap( 0, PlayerVelocityLimit, 0, 1, true ) * VelocitySpreadScale;

		if ( !Weapon.PlayerController.IsGrounded ) spread *= InAirSpreadMultiplier;

		return spread;
	}

	/// <summary>
	/// Runs a trace with all the data we have supplied it, and returns the result
	/// </summary>
	/// <returns></returns>
	protected virtual IEnumerable<SceneTraceResult> GetShootTrace()
	{
		float curHits = 0;
		var hits = new List<SceneTraceResult>();

		var start = WeaponRay.Position;
		var rot = Rotation.LookAt( WeaponRay.Forward );

		var forward = rot.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * GetBulletSpread() * 0.25f;
		forward = forward.Normal;

		var end = WeaponRay.Position + forward * MaxRange;
		while ( curHits < RicochetMaxHits )
		{
			curHits++;

			var tr = DoTraceBullet( start, end, BulletSize );
			if ( tr.Hit ) hits.Add( tr );

			var reflectDir = CalculateRicochetDirection( tr, ref curHits );
			var angle = reflectDir.Angle( tr.Direction );
			start = tr.EndPosition;
			end = tr.EndPosition + ( reflectDir * MaxRange );

			if ( !ShouldBulletContinue( tr, angle ) )
				break;
		}

		return hits;
	}

	/// <summary>
	/// Should we ricochet?
	/// </summary>
	/// <param name="tr"></param>
	/// <param name="angle"></param>
	/// <returns></returns>
	protected virtual bool ShouldBulletContinue( SceneTraceResult tr, float angle )
	{
		float maxAngle = MaxRicochetAngle;

		if ( angle > maxAngle )
			return false;

		return true;
	}

	protected float RPMToSeconds()
	{
		return 60 / FireRate;
	}

	/// <summary>
	/// Can we shoot this gun right now?
	/// </summary>
	/// <returns></returns>
	public bool CanShoot()
	{
		// Do we still have a weapon?
		if ( !Weapon.IsValid() ) return false;
		if ( !Weapon.PlayerController.IsValid() ) return false;
		
		// Player
		if ( Weapon.PlayerController.IsFrozen )
			return false;

		// Weapon
		if ( Weapon.Tags.Has( "reloading" ) || Weapon.Tags.Has( "no_shooting" ) )
			return false;

		// Delay checks
		if ( TimeSinceShoot < RPMToSeconds() )
			return false;

		// Ammo checks
		if ( RequiresAmmoComponent && ( AmmoComponent == null || !AmmoComponent.HasAmmo ) )
			return false;

		return true;
	}

	protected override void OnInput()
	{
		if ( CanShoot() )
		{
			Shoot();
		}
		else
		{
			// Dry fire
			if ( !AmmoComponent.HasAmmo )
			{
				if ( TimeSinceShoot < DryFireDelay )
					return;

				DryShoot();
			}
		}
	}
}
