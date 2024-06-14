using Sandbox.Events;

namespace Facepunch;

public record WeaponShotEvent : IGameEvent;

[Icon( "track_changes" )]
[Title( "Bullet" ), Group( "Weapon Components" )]
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


	[Property, Group( "Effects" )] public GameObject MuzzleFlashPrefab { get; set; }

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
	/// How far will we trace away from a gunshot wound, to make blood splatters?
	/// </summary>
	[Property, Group( "Effects" )] public float BloodEjectDistance { get; set; } = 512f;

	/// <summary>
	/// Accessor for the aim ray.
	/// </summary>
	protected Ray WeaponRay => Equipment.PlayerController.AimRay;

	/// <summary>
	/// How long since we shot?
	/// </summary>
	public TimeSince TimeSinceShoot { get; private set; }

	/// <summary>
	/// Fetches the desired model renderer that we'll focus effects on like trail effects, muzzle flashes, etc.
	/// </summary>
	protected IEquipment Effector
	{
		get
		{
			if ( IsProxy || !Equipment.ViewModel.IsValid() )
				return Equipment;

			return Equipment.ViewModel;
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
		if ( !Effector.ModelRenderer.IsValid() )
			return;

		// Create a muzzle flash from a GameObject / prefab
		if ( MuzzleFlashPrefab.IsValid() )
		{
			if ( Effector.Muzzle.IsValid() )
			{
				MuzzleFlashPrefab.Clone( new CloneConfig()
				{
					Parent = Effector.Muzzle,
					Transform = new(),
					StartEnabled = true
				} );
			}
		}

		if ( ShootSound is not null )
		{
			if ( Sound.Play( ShootSound, Equipment.Transform.Position ) is { } snd )
			{
				snd.ListenLocal = Equipment.PlayerController?.IsViewer ?? false;
				Log.Trace( $"ShootWeaponComponent: ShootSound {ShootSound.ResourceName}" );
			}
		}

		// Third person
		Equipment.PlayerController?.BodyRenderer.Set( "b_attack", true );

		// First person
		Equipment.ViewModel?.ModelRenderer.Set( "b_attack", true );
	}

	private LegacyParticleSystem CreateParticleSystem( string particle, Vector3 pos, Rotation rot, float decay = 5f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Transform.Position = pos;
		gameObject.Transform.Rotation = rot;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = ParticleSystem.Load( particle );
		gameObject.Transform.ClearInterpolation();

		// Clean these up between rounds
		gameObject.Components.Create<DestroyBetweenRounds>();

		// Clear off in a suitable amount of time.
		gameObject.DestroyAsync( decay );

		return p;
	}

	[Broadcast]
	private void CreateBloodEffects( Vector3 pos, Vector3 normal, Vector3 direction )
	{
		if ( !IsNearby( pos ) )
			return;

		// TODO: move this to the player
		var tr = Scene.Trace.Ray( pos, pos + direction * BloodEjectDistance )
			.WithoutTags( "player" )
			.Run();

		if ( tr.Hit )
		{
			var material = Game.Random.FromList( GetGlobal<PlayerGlobals>().BloodDecalMaterials );
			CreateDecal( material, tr.HitPosition - (tr.Direction * 2 ), tr.Normal, Game.Random.Float( 0, 360 ), Game.Random.Int( 32, 96 ), 10f, 30f );
		}
	}

	private DecalRenderer CreateDecal( Material material, Vector3 pos, Vector3 normal, float rotation, float size, float depth, float destroyTime = 3f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Transform.Position = pos;
		gameObject.Transform.Rotation = Rotation.LookAt( -normal );

		// Random rotation
		gameObject.Transform.Rotation *= Rotation.FromAxis( Vector3.Forward, rotation );

		var decalRenderer = gameObject.Components.Create<DecalRenderer>();
		decalRenderer.Material = material;
		decalRenderer.Size = new( size, size, depth );

		// Clean these up between rounds
		GameObject.Components.Create<DestroyBetweenRounds>();

		// Creates a destruction component to destroy the gameobject after a while
		gameObject.DestroyAsync( destroyTime );

		return decalRenderer;
	}

	private void CreateImpactEffects( Surface surface, Vector3 pos, Vector3 normal )
	{
		var decalPath = Game.Random.FromList( surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );
		if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalResource ) )
		{
			var decal = Game.Random.FromList( decalResource.Decals );

			CreateDecal( decal.Material, pos, normal, decal.Rotation.GetValue(), decal.Width.GetValue() / 1.5f, decal.Depth.GetValue(), 30f );
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

		if ( AmmoComponent is not null )
		{
			AmmoComponent.Ammo--;
		}

		DoShootEffects();

		GameObject.Dispatch( new WeaponShotEvent() );

		for ( int i = 0; i < BulletCount; i++ )
		{
			int count = 0;

			foreach ( var tr in GetShootTrace() )
			{
				if ( !tr.Hit )
					continue;

				CreateImpactEffects( tr.Surface, tr.EndPosition, tr.Normal );
				DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance, count );

				if ( tr.GameObject?.Root.Components.Get<PlayerController>( FindMode.EnabledInSelfAndDescendants ) is { } player )
				{
					CreateBloodEffects( tr.HitPosition, tr.Normal, tr.Direction );
				}

				var damage = CalculateDamageFalloff( BaseDamage, tr.Distance );
				damage = damage.CeilToInt();

				// Inflict damage on whatever we find.

				using ( Rpc.FilterInclude( Connection.Host ) )
				{
					InflictDamage( tr.GameObject!.Id, damage, tr.EndPosition, tr.Direction, tr.GetHitboxTags() );
				}

				count++;
			}
		}

		// If we have a recoil function, let it know.
		Equipment.Components.Get<RecoilWeaponComponent>( FindMode.EnabledInSelfAndDescendants )?.Shoot();
	}

	[Broadcast]
	private void InflictDamage( Guid targetObjectId, float damage, Vector3 pos, Vector3 dir, HitboxTags hitbox )
	{
		var target = Scene.Directory.FindByGuid( targetObjectId );

		// target?.TakeDamage( damage, tr.EndPosition, tr.Direction * tr.Distance, Weapon.PlayerController.HealthComponent.Id, Weapon.Id, hitbox );
		target?.TakeDamage( new DamageInfo( Equipment.PlayerController, damage, Equipment, pos, dir * damage, hitbox ) );
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

		var origin = count == 0 ? Effector?.Muzzle?.Transform.Position ?? Equipment.Transform.Position : startPosition;
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
			var snd = Sound.Play( DryFireSound, Equipment.Transform.Position );
			snd.ListenLocal = !IsProxy;
			Log.Trace( $"ShootWeaponComponent: ShootSound {DryFireSound.ResourceName}" );
		}

		// First person
		Equipment.ViewModel?.ModelRenderer.Set( "b_attack_dry", true );
	}

	protected SceneTraceResult DoTraceBullet( Vector3 start, Vector3 end, float radius )
	{
		return Scene.Trace.Ray( start, end )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "ragdoll", "movement" )
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
		var rot = Rotation.LookAt( WeaponRay.Forward );

		var forward = rot.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * ( BulletSpread + Equipment.PlayerController.Spread ) * 0.25f;
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
		if ( !Equipment.IsValid() ) return false;
		if ( !Equipment.PlayerController.IsValid() ) return false;
		
		// Player
		if ( Equipment.PlayerController.IsFrozen )
			return false;

		// Weapon
		if ( Equipment.Tags.Has( "reloading" ) || Equipment.Tags.Has( "no_shooting" ) )
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
