using Sandbox.Events;

namespace Facepunch;

public record WeaponShotEvent : IGameEvent;

public enum FireMode
{
	Semi,
	Automatic,
	Burst
}

[Icon( "track_changes" )]
[Title( "Bullet" ), Group( "Weapon Components" )]
public partial class Shootable : WeaponInputAction
{
	[Property, Group( "Bullet" ), EquipmentResourceProperty] public float BaseDamage { get; set; } = 25.0f;
	[Property, Group( "Bullet" ), EquipmentResourceProperty] public float FireRate { get; set; } = 0.2f;
	[Property, Group( "Bullet" )] public float DryShootDelay { get; set; } = 0.15f;
	[Property, Group( "Bullet" )] public float BulletSize { get; set; } = 1.0f;
	[Property, Group( "Bullet" ), EquipmentResourceProperty] public int BulletCount { get; set; } = 1;

	[Property, Group( "Bullet Falloff" ), EquipmentResourceProperty] public Curve BaseDamageFalloff { get; set; } = new( new List<Curve.Frame>() { new( 0, 1 ), new( 1, 0 ) } );
	[Property, Group( "Bullet Falloff" ), EquipmentResourceProperty] public float MaxRange { get; set; } = 1024000;

	[Property, Group( "Bullet Spread" )] public float BulletSpread { get; set; } = 0;
	[Property, Group( "Bullet Spread" )] public float PlayerVelocityLimit { get; set; } = 300f;
	[Property, Group( "Bullet Spread" )] public float VelocitySpreadScale { get; set; } = 0.25f;
	[Property, Group( "Bullet Spread" )] public float InAirSpreadMultiplier { get; set; } = 2f;

	[Property, Group( "Penetration" )] public float PenetrationThickness { get; set; } = 32f;

	[Property, Group( "Effects" )] public GameObject MuzzleFlashPrefab { get; set; }
	[Property, Group( "Effects" )] public GameObject EjectionPrefab { get; set; }

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
	[Property, Category( "Ammo" ), Feature( "Ammo" )] public WeaponAmmo AmmoComponent { get; set; }

	/// <summary>
	/// Does this weapon require an ammo container to fire its bullets?
	/// </summary>
	[Property, Category( "Ammo" ), FeatureEnabled( "Ammo" )] public bool RequiresHasAmmo { get; set; } = false;

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
	/// How quickly can we switch fire mode?
	/// </summary>
	[Property, Group( "Fire Modes" )] public float FireModeSwitchDelay { get; set; } = 0.3f;

	/// <summary>
	/// What fire modes do we support?
	/// </summary>
	[Property, Group( "Fire Modes" )]
	public List<FireMode> SupportedFireModes { get; set; } = new()
	{
		FireMode.Automatic
	};

	/// <summary>
	/// What's our current fire mode? (Or Default)
	/// </summary>
	[Property, Sync, Group( "Fire Modes" )] public FireMode CurrentFireMode { get; set; } = FireMode.Automatic;

	/// <summary>
	/// How many bullets describes a burst?
	/// </summary>
	[Property, Group( "Fire Modes" )] public int BurstAmount { get; set; } = 3;

	/// <summary>
	/// How long after we finish a burst until we can shoot again?
	/// </summary>
	[Property, Group( "Fire Modes" )] public float BurstEndDelay { get; set; } = 0.2f;

	[Sync] public TimeSince TimeSinceFireModeSwitch { get; set; }
	[Sync] public TimeSince TimeSinceBurstFinished { get; set; }
	[Sync] public bool IsBurstFiring { get; set; }

	/// <summary>
	/// Accessor for the aim ray.
	/// </summary>
	protected Ray WeaponRay => Equipment.Owner.AimRay;

	/// <summary>
	/// How long since we shot?
	/// </summary>
	public TimeSince TimeSinceShoot { get; private set; }

	/// <summary>
	/// Fetches the desired model renderer that we'll focus effects on like trail effects, muzzle flashes, etc.
	/// </summary>
	protected WeaponModel Effector
	{
		get
		{
			if ( IsProxy || !Equipment.ViewModel.IsValid() )
				return Equipment.WorldModel;

			return Equipment.ViewModel;
		}
	}

	protected override void OnEnabled()
	{
		BindTag( "no_ammo", () => AmmoComponent.IsValid() && !AmmoComponent.HasAmmo );
	}

	/// <summary>
	/// Play any particle effects such as muzzle flashes.
	/// </summary>
	[Rpc.Broadcast]
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
					StartEnabled = true,
					Name = $"Muzzle flash: {Equipment.GameObject}"
				} );
			}
		}

		// Eject casing using GameObject / prefab
		if ( EjectionPrefab.IsValid() )
		{
			if ( Effector.EjectionPort.IsValid() )
			{
				EjectionPrefab.Clone( new CloneConfig()
				{
					Parent = Effector.EjectionPort,
					Transform = new(),
					StartEnabled = true,
					Name = $"Bullet ejection: {Equipment.GameObject}"
				} );
			}
		}

		if ( ShootSound is not null )
		{
			if ( Sound.Play( ShootSound, Equipment.WorldPosition ) is { } snd )
			{
				snd.SpacialBlend = (Equipment.Owner?.IsViewer ?? false) ? 0 : snd.SpacialBlend;
				Log.Trace( $"Shootable: ShootSound {ShootSound.ResourceName}" );
			}
		}

		// Third person
		if ( Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
			Equipment.Owner.BodyRenderer.Set( "b_attack", true );

		// First person
		if ( Equipment.ViewModel.IsValid() )
			Equipment.ViewModel.ModelRenderer.Set( "b_attack", true );
	}

	private Decal CreateDecal( Texture material, Vector3 pos, Vector3 normal, float rotation, float size, float depth, float destroyTime = 3f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Name = $"Impact decal: {Equipment.GameObject}";
		gameObject.WorldPosition = pos;
		gameObject.WorldRotation = Rotation.LookAt( -normal );

		// Random rotation
		gameObject.WorldRotation *= Rotation.FromAxis( Vector3.Forward, rotation );

		var decal = gameObject.Components.Create<Decal>();
		decal.Decals.Add( new DecalDefinition()
		{
			ColorTexture = material,
			Height = size,
			Width = size
		} );

		// Clean these up between rounds
		gameObject.Components.Create<DestroyBetweenRounds>();

		// Creates a destruction component to destroy the gameobject after a while
		gameObject.DestroyAsync( destroyTime );

		return decal;
	}

	private void CreateImpactEffects( GameObject hitObject, Surface surface, Vector3 pos, Vector3 normal )
	{
		var impactPrefab = surface.GetBulletImpact();

		if ( impactPrefab is not null )
		{
			var impact = impactPrefab.Clone();
			impact.WorldPosition = pos + normal;
			impact.WorldRotation = Rotation.LookAt( normal );
			impact.SetParent( hitObject, true );
		}

		if ( surface.SoundCollection.Bullet.IsValid() )
		{
			Sound.Play( surface.SoundCollection.Bullet, pos );
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

		if ( CurrentFireMode == FireMode.Burst )
		{
			IsBurstFiring = true;
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

				if ( tr.Distance == 0 )
					continue;

				CreateImpactEffects( tr.GameObject, tr.Surface, tr.EndPosition, tr.Normal );
				DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance, count );

				var damage = CalculateDamageFalloff( BaseDamage, tr.Distance );
				damage = damage.CeilToInt();

				// Inflict damage on whatever we find.

				var damageFlags = DamageFlags.None;
				if ( count > 0 ) damageFlags |= DamageFlags.WallBang;
				if ( !Player.IsGrounded ) damageFlags |= DamageFlags.AirShot;

				using ( Rpc.FilterInclude( Connection.Host ) )
				{
					InflictDamage( tr.GameObject, damage, tr.EndPosition, tr.Direction, tr.GetHitboxTags(), damageFlags );
				}

				count++;
			}
		}

		// If we have a recoil function, let it know.
		var recoil = Equipment.GetComponentInChildren<ShootRecoil>();
		if ( recoil.IsValid() )
			recoil.Shoot();
	}

	[Rpc.Broadcast]
	private void InflictDamage( GameObject target, float damage, Vector3 pos, Vector3 dir, HitboxTags hitbox, DamageFlags flags )
	{
		if ( target.IsValid() )
		{
			target.TakeDamage( new DamageInfo( Equipment.Owner, damage, Equipment, pos, dir * damage, hitbox, flags ) );
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
		if ( !Scene.Camera.IsValid() ) return false;
		return position.DistanceSquared( Scene.Camera.WorldPosition ) < MaxEffectsPlayDistance;
	}

	[Property]
	public GameObject TracerEffect { get; set; }

	private GameObject Tracer
	{
		get
		{
			if ( TracerEffect.IsValid() ) return TracerEffect;

			return GameObject.GetPrefab( $"/weapons/common/effects/tracer_9mm.prefab" );
		}
	}

	/// <summary>
	/// Makes some tracers using legacy particle effects.
	/// </summary>
	[Rpc.Broadcast]
	protected void DoTracer( Vector3 startPosition, Vector3 endPosition, float distance, int count )
	{
		if ( !IsNearby( startPosition ) && !IsNearby( endPosition ) ) return;

		var origin = count == 0 ? Effector?.Muzzle?.WorldPosition ?? Equipment.WorldPosition : startPosition;

		var effect = Tracer?.Clone( new CloneConfig { Transform = new Transform().WithPosition( origin ), StartEnabled = true } );
		if ( effect.IsValid() && effect.GetComponentInChildren<Tracer>() is Tracer tracer )
		{
			tracer.EndPoint = endPosition;
		}
	}

	protected void DryShoot()
	{
		TimeSinceShoot = 0f;
		DryShootEffects();
	}

	[Rpc.Broadcast]
	protected void DryShootEffects()
	{
		if ( DryFireSound is not null )
		{
			var snd = Sound.Play( DryFireSound, Equipment.WorldPosition );
			snd.SpacialBlend = (Equipment.Owner?.IsViewer ?? false) ? 0 : snd.SpacialBlend;
			Log.Trace( $"Shootable: ShootSound {DryFireSound.ResourceName}" );
		}

		// First person
		Equipment.ViewModel?.ModelRenderer.Set( "b_attack_dry", true );
	}

	protected IEnumerable<SceneTraceResult> DoTraceBullet( Vector3 start, Vector3 end, float radius )
	{
		return Scene.Trace.Ray( start, end )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "playerclip", "movement" )
			.Size( radius )
			.RunAll();
	}


	protected SceneTraceResult DoTraceBulletOne( Vector3 start, Vector3 end, float radius )
	{
		return Scene.Trace.Ray( start, end )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "playerclip", "movement" )
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
		var hits = new List<SceneTraceResult>();

		var start = WeaponRay.Position;
		var rot = Rotation.LookAt( WeaponRay.Forward );

		var forward = rot.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * (BulletSpread + Equipment.Owner.Spread) * 0.25f;
		forward = forward.Normal;

		var original = DoTraceBullet( start, WeaponRay.Position + forward * MaxRange, BulletSize );

		if ( original.Count() < 1 ) return original;

		// Run through and fix the start positions for the traces
		// By using the last end position as the start

		int depth = 0;
		Vector3 startPos = original.ElementAt( 0 ).StartPosition;
		List<SceneTraceResult> fixedPath = new();
		for ( int i = 0; i < original.Count(); i++ )
		{
			var el = original.ElementAt( i );

			fixedPath.Add( el with { StartPosition = startPos } );
			startPos = el.EndPosition;
		}

		var entries = new List<(SceneTraceResult Trace, float Thickness)>();

		// Then, trace backwards from the end so we can get exit points and thickness
		for ( int i = fixedPath.Count - 1; i >= 0; i-- )
		{
			var el = fixedPath.ElementAt( i );

			// Do a trace back, from the end position to the start, this'll give us the LAST entry's exit point.
			var backTrace = DoTraceBulletOne( el.EndPosition, el.StartPosition, BulletSize );
			var impact = backTrace.EndPosition;

			// From that, we can calculate the surface thickness
			float thickness = (el.StartPosition - impact).Length;

			// Return the element starting at the exit point, it's more useful that way.
			el = el with { StartPosition = impact };
			entries.Insert( 0, (el, thickness) );
		}

		depth = 0;
		float accThickness = 0;
		foreach ( var el in entries )
		{
			accThickness += el.Thickness;
			if ( accThickness >= PenetrationThickness )
				break;

			hits.Add( el.Trace );
			// DrawLineSegment( el.Trace.StartPosition, el.Trace.EndPosition, depth, fixedPath.Count() );
			depth++;
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
		if ( !Equipment.Owner.IsValid() ) return false;

		// Player
		if ( Equipment.Owner.IsFrozen )
			return false;

		// Weapon
		if ( Equipment.Tags.Has( "reloading" ) || Equipment.Tags.Has( "no_shooting" ) )
			return false;

		// Delay checks
		if ( TimeSinceShoot < RPMToSeconds() )
			return false;

		// Ammo checks
		if ( RequiresHasAmmo && (!AmmoComponent.IsValid() || !AmmoComponent.HasAmmo) )
			return false;

		return true;
	}

	[Sync] public int BurstCount { get; set; } = 0;

	private void ClearBurst()
	{
		TimeSinceBurstFinished = 0;
		IsBurstFiring = false;
		BurstCount = 0;
	}

	protected override void OnInputUpdate()
	{
		if ( Input.Pressed( "FireMode" ) )
		{
			CycleFireMode();
			return;
		}

		if ( IsBurstFiring && BurstCount >= BurstAmount - 1 || (Tags.Has( "reloading" ) && IsBurstFiring) )
		{
			ClearBurst();
		}

		if ( CurrentFireMode == FireMode.Burst && IsBurstFiring && CanShoot() )
		{
			BurstCount++;
			Shoot();
		}

		bool wantsToShoot = IsDown();

		// HACK
		if ( CurrentFireMode == FireMode.Semi )
		{
			wantsToShoot = Input.Pressed( "attack1" );
		}

		if ( wantsToShoot )
		{
			if ( Equipment.HasTag( "lowered" ) )
			{
				Equipment.SetTag( "lowered", false );
				return;
			}

			if ( !CanShoot() )
			{
				// Dry fire
				if ( !AmmoComponent.HasAmmo )
				{
					if ( TimeSinceShoot < DryShootDelay )
						return;

					if ( Tags.Has( "reloading" ) )
						return;

					DryShoot();
				}
			}
			else
			{
				if ( IsBurstFiring ) return;
				if ( TimeSinceBurstFinished < BurstEndDelay ) return;

				Shoot();
			}
		}
	}

	protected int GetFireModeIndex( FireMode fireMode )
	{
		int i = 0;
		foreach ( var mode in SupportedFireModes )
		{
			if ( mode == fireMode ) return i;
			i++;
		}

		return 0;
	}

	public void CycleFireMode()
	{
		if ( TimeSinceFireModeSwitch < FireModeSwitchDelay ) return;
		if ( IsBurstFiring ) return;
		if ( IsDown() ) return;

		var curIndex = GetFireModeIndex( CurrentFireMode );
		var length = SupportedFireModes.Count;
		var newIndex = (curIndex + 1 + length) % length;

		// We didn't change anything
		if ( newIndex == curIndex ) return;

		CurrentFireMode = SupportedFireModes[newIndex];

		Equipment.ViewModel?.OnFireMode( CurrentFireMode );

		// Toast.Instance?.Show( $"{CurrentFireMode}", ToastType.Generic, 1f );

		TimeSinceFireModeSwitch = 0;
	}

	protected override void OnEquipmentHolstered()
	{
		ClearBurst();
	}
}
