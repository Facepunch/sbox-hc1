using System.ComponentModel;
using System.Diagnostics;

namespace Facepunch;

[Icon( "track_changes" )]
public partial class ShootWeaponFunction : InputActionWeaponFunction
{
	[Property, Category( "Bullet" )] public float BaseDamage { get; set; } = 25.0f;
	[Property, Category( "Bullet" )] public float FireRate { get; set; } = 0.2f;
	[Property, Category( "Bullet" )] public float DryFireDelay { get; set; } = 1f;
	[Property, Category( "Bullet" )] public float MaxRange { get; set; } = 1024000;
	[Property, Category( "Bullet" )] public Curve BaseDamageFalloff { get; set; } = new( new List<Curve.Frame>() { new( 0, 1 ), new( 1, 0 ) } );
	[Property, Category( "Bullet" )] public float BulletSize { get; set; } = 1.0f;

	// Effects
	[Property, Category( "Effects" )] public GameObject MuzzleFlash { get; set; }
	[Property, Category( "Effects" )] public GameObject BulletTrail { get; set; }
	[Property, Category( "Effects" )] public SoundEvent ShootSound { get; set; }
	[Property, Category( "Effects" )] public SoundEvent DryFireSound { get; set; }

	/// <summary>
	/// The current weapon's ammo container.
	/// </summary>
	[Property, Category( "Ammo" )] public AmmoContainer AmmoContainer { get; set; }

	/// <summary>
	/// Does this weapon require an ammo container to fire its bullets?
	/// </summary>
	[Property, Category( "Ammo" )] public bool RequiresAmmoContainer { get; set; } = false;

	/// <summary>
	/// Fetches the desired model renderer that we'll focus effects on like trail effects, muzzle flashes, etc.
	/// </summary>
	protected SkinnedModelRenderer EffectsRenderer 
	{
		get
		{
			if ( IsProxy || !Weapon.ViewModel.IsValid() )
			{
				return Weapon.ModelRenderer;
			}

			return Weapon.ViewModel.ModelRenderer;
		}
	}

	public TimeSince TimeSinceShoot { get; private set; }

	/// <summary>
	/// Do shoot effects
	/// </summary>
	[Broadcast]
	protected void DoShootEffects()
	{
		if ( !EffectsRenderer.IsValid() )
		{
			return;
		}

		// Create a muzzle flash from a GameObject / prefab
		if ( MuzzleFlash.IsValid() )
		{
			// SceneUtility.Instantiate( MuzzleFlash, EffectsRenderer.GetAttachment( "muzzle" ) ?? Weapon.Transform.World );
		}

		if ( ShootSound is not null )
		{
			if ( Sound.Play( ShootSound, Weapon.Transform.Position ) is SoundHandle snd )
			{
				snd.ListenLocal = !IsProxy;
				Log.Trace( $"ShootWeaponFunction: ShootSound {ShootSound.ResourceName}" );
			}
		}

		// Third person
		Weapon.PlayerController.BodyRenderer.Set( "b_attack", true );

		// First person
		Weapon.ViewModel?.ModelRenderer.Set( "b_attack", true );
	}

	/// <summary>
	/// Gets a surface from a trace.
	/// </summary>
	/// <param name="tr"></param>
	/// <returns></returns>
	private Surface GetSurfaceFromTrace( SceneTraceResult tr )
	{
		return tr.Surface;	
	}

	private LegacyParticleSystem CreateParticleSystem( string particle, Vector3 pos, Rotation rot, float decay = 5f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Transform.Position = pos;
		gameObject.Transform.Rotation = rot;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = ParticleSystem.Load( particle );

		// ?
		gameObject.Transform.ClearInterpolation();

		// Clear off in a suitable amount of time.
		gameObject.DestroyAsync( decay );

		return p;
	}

	[Broadcast]
	private void CreateImpactEffects( Surface surface, Vector3 pos, Vector3 normal )
	{
		var decalPath = Game.Random.FromList( surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );
		if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalResource ) )
		{
			//var ps = CreateParticleSystem( Game.Random.FromList( surface.ImpactEffects.Bullet ), pos, Rotation.LookAt( -normal ) );
			//ps.SceneObject.SetControlPoint( 0, pos );

			var decal = Game.Random.FromList( decalResource.Decals );

			var gameObject = Scene.CreateObject();
			gameObject.Transform.Position = pos;
			gameObject.Transform.Rotation = Rotation.LookAt( -normal );

			// Random rotation
			gameObject.Transform.Rotation *= Rotation.FromAxis( Vector3.Forward, decal.Rotation.GetValue() );

			var decalRenderer = gameObject.Components.Create<DecalRenderer>();
			decalRenderer.Material = decal.Material;
			decalRenderer.Size = new( decal.Width.GetValue(), decal.Height.GetValue(), decal.Depth.GetValue() );

			// Creates a destruction component to destroy the gameobject after a while
			gameObject.DestroyAsync( 3f );
		}

		if ( !string.IsNullOrEmpty( surface.Sounds.Bullet ) )
		{
			Sound.Play( surface.Sounds.Bullet, pos );
		}
	}

	/// <summary>
	/// Shoot the gun!
	/// </summary>
	public void Shoot()
	{
		TimeSinceShoot = 0;

		if ( AmmoContainer is not null )
		{
			AmmoContainer.Ammo--;
		}

		int count = 0;

		// If we have a recoil function, let it know.
		Weapon.GetFunction<RecoilFunction>()?.Shoot();

		foreach ( var tr in GetShootTrace() )
		{
			if ( !tr.Hit )
			{
				DoShootEffects();
				return;
			}

			DoShootEffects();
			CreateImpactEffects( GetSurfaceFromTrace( tr ), tr.EndPosition, tr.Normal );
			DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance, count );

			// Inflict damage on whatever we find.
			var damageInfo = DamageInfo.Bullet( BaseDamage, Weapon.PlayerController.GameObject, Weapon.GameObject );
			tr.GameObject.TakeDamage( ref damageInfo );
			count++;
		}
	}

	/// <summary>
	/// Makes some tracers using legacy particle effects.
	/// TODO: replace these with our new cool particle system.
	/// </summary>
	/// <param name="startPosition"></param>
	/// <param name="endPosition"></param>
	/// <param name="distance"></param>
	/// <param name="count"></param>
	protected void DoTracer( Vector3 startPosition, Vector3 endPosition, float distance, int count )
	{
		var effectPath = "particles/gameplay/guns/trail/trail_smoke.vpcf";

		// For when we have bullet penetration implemented.
		if ( count > 0 )
		{
			effectPath = "particles/gameplay/guns/trail/rico_trail_smoke.vpcf";

			// Project backward
			Vector3 dir = (startPosition - endPosition).Normal;
			var tr = Scene.Trace.Ray( endPosition, startPosition + (dir * 50f) )
				.Radius( 1f )
				.WithoutTags( "weapon" )
				.Run();

			if ( tr.Hit )
			{
				CreateImpactEffects( GetSurfaceFromTrace( tr ), tr.StartPosition, dir );
			}
		}

		var origin = count == 0 ? EffectsRenderer.GetAttachment( "muzzle" )?.Position ?? startPosition : startPosition;

		var ps = CreateParticleSystem( effectPath, origin, Rotation.Identity, 3f );
		ps.SceneObject.SetControlPoint( 0, origin );
		ps.SceneObject.SetControlPoint( 1, endPosition );
		ps.SceneObject.SetControlPoint( 2, distance );
	}

	protected void DryShoot()
	{
		TimeSinceShoot = 0;
		DryShootEffects();
	}

	protected void DryShootEffects()
	{
		if ( DryFireSound is not null )
		{
			var snd = Sound.Play( DryFireSound, Weapon.Transform.Position );
			snd.ListenLocal = !IsProxy;

			Log.Trace( $"ShootWeaponFunction: ShootSound {DryFireSound.ResourceName}" );
		}

		// First person
		Weapon.ViewModel?.ModelRenderer.Set( "b_attack_dry", true );
	}

	protected virtual Ray WeaponRay => Weapon.PlayerController.AimRay;

	/// <summary>
	/// How many ricochet hits until we stop traversing
	/// </summary>
	protected virtual float MaxAmtOfHits => 2f;

	/// <summary>
	/// Maximum angle in degrees for ricochet to be possible
	/// </summary>
	protected virtual float MaxRicochetAngle => 45f;

	protected float PenetrationIncrementAmount => 15f;
	protected int PenetrationMaxSteps => 2;

	protected SceneTraceResult DoTraceBullet( Vector3 start, Vector3 end, float radius )
	{
		return Scene.Trace.Ray( start, end )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger" )
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
	/// Runs a trace with all the data we have supplied it, and returns the result
	/// </summary>
	/// <returns></returns>
	protected virtual IEnumerable<SceneTraceResult> GetShootTrace()
	{
		float curHits = 0;
		var hits = new List<SceneTraceResult>();

		var start = WeaponRay.Position;
		var end = WeaponRay.Position + WeaponRay.Forward * MaxRange;

		while ( curHits < MaxAmtOfHits )
		{
			curHits++;

			var tr = DoTraceBullet( start, end, BulletSize );

			if ( tr.Hit )
			{
				hits.Add( tr );
			}

			var reflectDir = CalculateRicochetDirection( tr, ref curHits );
			var angle = reflectDir.Angle( tr.Direction );
			var dist = tr.Distance.Remap( 0, MaxRange, 1, 0.5f ).Clamp( 0.5f, 1f );

			start = tr.EndPosition;
			end = tr.EndPosition + (reflectDir * MaxRange);

			var didPenetrate = false;
			if ( true )
			{
				// Look for penetration
				var forwardStep = 0f;

				while ( forwardStep < PenetrationMaxSteps )
				{
					forwardStep++;

					var penStart = tr.EndPosition + tr.Direction * (forwardStep * PenetrationIncrementAmount);
					var penEnd = tr.EndPosition + tr.Direction * (forwardStep + 1 * PenetrationIncrementAmount);

					var penTrace = DoTraceBullet( penStart, penEnd, BulletSize );
					if ( !penTrace.StartedSolid )
					{
						var newStart = penTrace.EndPosition;
						var newTrace = DoTraceBullet( newStart, newStart + tr.Direction * MaxRange, BulletSize );
						hits.Add( newTrace );
						didPenetrate = true;
						break;
					}
				}
			}

			if ( didPenetrate || !ShouldBulletContinue( tr, angle ) )
				break;
		}

		return hits;
	}

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
		// Player
		if ( Weapon.PlayerController.HasTag( "sprint" ) || Weapon.PlayerController.IsFrozen )
			return false;

		// Weapon
		if ( Weapon.Tags.Has( "reloading" ) || Weapon.Tags.Has( "no_shooting" ) )
			return false;

		// Delay checks
		if ( TimeSinceShoot < RPMToSeconds() )
		{
			return false;
		}

		// Ammo checks
		if ( RequiresAmmoContainer && ( AmmoContainer == null || !AmmoContainer.HasAmmo ) )
		{
			return false;
		}

		return true;
	}

	protected override void OnFunctionExecute()
	{
		if ( CanShoot() )
		{
			Shoot();
		}
		else
		{
			// Dry fire
			if ( !AmmoContainer.HasAmmo )
			{
				if ( TimeSinceShoot < DryFireDelay )
					return;

				DryShoot();
			}
		}
	}
}
