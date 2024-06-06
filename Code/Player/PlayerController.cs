using Facepunch.UI;
using Sandbox.Diagnostics;
using System.Diagnostics;

namespace Facepunch;

public partial class PlayerController : Component, IPawn, IRespawnable, IDamageListener, Weapon.IDeploymentListener
{
	/// <summary>
	/// Sync the player's steamid
	/// </summary>
	[Sync] public ulong SteamId { get; set; }

	/// <summary>
	/// The player's body
	/// </summary>
	[Property] public PlayerBody Body { get; set; }

	/// <summary>
	/// A reference to the player's head (the GameObject)
	/// </summary>
	[Property] public GameObject Head { get; set; }

	/// <summary>
	/// A reference to the animation helper (normally on the Body GameObject)
	/// </summary>
	[Property] public AnimationHelper AnimationHelper { get; set; }

	/// <summary>
	/// The current gravity. Make this a gamerule thing later?
	/// </summary>
	[Property, Group( "Config" )] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	/// <summary>
	/// What effect should we spawn when a player gets headshot?
	/// </summary>
	[Property, Group( "Effects" )] public GameObject HeadshotEffect { get; set; }

	/// <summary>
	/// What effect should we spawn when a player gets headshot while wearing a helmet?
	/// </summary>
	[Property, Group( "Effects" )] public GameObject HeadshotWithHelmetEffect { get; set; }

	/// <summary>
	/// What effect should we spawn when we hit a player?
	/// </summary>
	[Property, Group( "Effects" )] public GameObject BloodEffect { get; set; }

	/// <summary>
	/// What sound should we play when a player gets headshot?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent HeadshotSound { get; set; }

	/// <summary>
	/// What sound should we play when a player gets headshot?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent HeadshotWithHelmetSound { get; set; }

	/// <summary>
	/// What sound should we play when we hit a player?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent BloodImpactSound { get; set; }

	/// <summary>
	/// The current character controller for this player.
	/// </summary>
	[RequireComponent] public CharacterController CharacterController { get; set; }

	/// <summary>
	/// The current camera controller for this player.
	/// </summary>
	[RequireComponent] public CameraController CameraController { get; set; }
	
	/// <summary>
	/// The outline effect for this player.
	/// </summary>
	[RequireComponent] public HighlightOutline Outline { get; set; }

	/// <summary>
	/// A reference to the View Model's camera. This will be disabled by the View Model.
	/// </summary>
	[Property] public CameraComponent ViewModelCamera { get; set; }

	/// <summary>
	/// Handles the player's outfit
	/// </summary>
	[Property] public PlayerOutfitter Outfitter { get; set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold our ViewModel.
	/// </summary>
	[Property] public GameObject ViewModelGameObject { get; set; }

	/// <summary>
	/// Get a quick reference to the real Camera GameObject.
	/// </summary>
	public GameObject CameraGameObject => CameraController.Camera.GameObject;

	/// <summary>
	/// Finds the first <see cref="SkinnedModelRenderer"/> on <see cref="Body"/>
	/// </summary>
	public SkinnedModelRenderer BodyRenderer => Body.Components.Get<SkinnedModelRenderer>();

	/// <summary>
	/// An accessor to get the camera controller's aim ray.
	/// </summary>
	public Ray AimRay => CameraController.AimRay;

	// TODO: move this into something that isn't on the player, this should be on an animator being fed info like the weapon
	[Sync] AnimationHelper.HoldTypes CurrentHoldType { get; set; } = AnimationHelper.HoldTypes.None;

	/// <summary>
	/// Called when the player jumps.
	/// </summary>
	[Property] public Action OnJump { get; set; }

	/// <summary>
	/// How quickly does the player move by default?
	/// </summary>
	[Property, Group( "Config" )] public float WalkSpeed { get; set; } = 125f;

	/// <summary>
	/// How powerful is the player's jump?
	/// </summary>
	[Property, Group( "Config" )] public float JumpPower { get; set; } = 320f;

	/// <summary>
	/// How much friction does the player have?
	/// </summary>
	[Property, Group( "Friction" )] public float BaseFriction { get; set; } = 4.0f;


	[Property, Group( "Friction" )] public float SlowWalkFriction { get; set; } = 4.0f;
	[Property, Group( "Friction" )] public float CrouchingFriction { get; set; } = 4.0f;

	/// <summary>
	/// Can we control our movement in the air?
	/// </summary>
	[Property, Group( "Acceleration" )] public float AirAcceleration { get; set; } = 40f;

	[Property, Group( "Acceleration" )] public float BaseAcceleration { get; set; } = 10;
	[Property, Group( "Acceleration" )] public float SlowWalkAcceleration { get; set; } = 10;
	[Property, Group( "Acceleration" )] public float CrouchingAcceleration { get; set; } = 10;

	/// <summary>
	/// Is the player crouching?
	/// </summary>
	[Sync] public bool IsCrouching { get; set; }

	/// <summary>
	/// Is the player slow walking?
	/// </summary>
	[Sync] public bool IsSlowWalking { get; set; }

	/// <summary>
	/// Is the player noclipping?
	/// </summary>
	[Sync] public bool IsNoclipping { get; set; }

	/// <summary>
	/// Noclip movement speed
	/// </summary>
	[Property] public float NoclipSpeed { get; set; } = 1000f;

	/// <summary>
	/// The player's box collider, so people can jump on other people.
	/// </summary>
	[Property] public BoxCollider PlayerBoxCollider { get; set; }

	[Property, Group( "Config" )] public float Height { get; set; } = 64f;

	/// <summary>
	/// How far can we use stuff?
	/// </summary>
	[Property, Group( "Interaction" )] public float UseDistance { get; set; } = 72f;

	/// <summary>
	/// A shorthand accessor to say if we're controlling this player.
	/// </summary>
	public bool IsLocallyControlled => IsViewer && !IsProxy && !IsBot;

	/// <summary>
	/// Is this player the currently possessed controller
	/// </summary>
	public bool IsViewer => (this as IPawn).IsPossessed;

	/// <summary>
	/// Unique ID of this Bot
	/// </summary>
	[HostSync] public int BotId { get; set; } = -1;

	[ConVar( "hc1_bot_follow" )] public static bool BotFollowHostInput { get; set; }

	/// <summary>
	/// Is this a player or a bot
	/// </summary>
	public bool IsBot => BotId != -1;

	public string GetPlayerName() => IsBot ? $"BOT {BotManager.Instance.GetName(BotId)}" : Network.OwnerConnection?.DisplayName ?? "";

	/// <summary>
	/// If true, we're not allowed to move.
	/// </summary>
	[HostSync] public bool IsFrozen { get; set; }

	/// <summary>
	/// Last time this player moved or attacked.
	/// </summary>
	[Sync] public TimeSince TimeSinceLastInput { get; private set; }

	private Vector3 WishVelocity { get; set; }
	public bool IsGrounded { get; set; }
	public Vector3 WishMove { get; private set; }
	public bool InBuyMenu { get; private set; }
	/// <summary>
	/// How much friction to apply to the aim eg if zooming
	/// </summary>
	public float AimDampening = 1.0f;
	public bool InMenu => InBuyMenu;
	
	private float _smoothEyeHeight;

	[Sync] public Angles EyeAngles { get; set; }
	
	/// <summary>
	/// What weapon are we using?
	/// </summary>
	[Property, ReadOnly] public Weapon CurrentWeapon { get; private set; }

	void Weapon.IDeploymentListener.OnDeployed( Weapon weapon )
	{
		CurrentWeapon = weapon;
	}

	void Weapon.IDeploymentListener.OnHolstered( Weapon weapon )
	{
		if ( weapon == CurrentWeapon )
			CurrentWeapon = null;
	}

	[Authority]
	private void SetCurrentWeapon( Guid weaponId )
	{
		var weapon = Scene.Directory.FindComponentByGuid( weaponId ) as Weapon;
		SetCurrentWeapon( weapon );
	}

	[Authority]
	private void ClearCurrentWeapon()
	{
		CurrentWeapon?.Holster();
	}

	public void Holster()
	{
		if ( IsProxy )
		{
			if ( Networking.IsHost )
				ClearCurrentWeapon();

			return;
		}

		CurrentWeapon?.Holster();
	}

	public void SetCurrentWeapon( Weapon weapon )
	{
		if ( IsProxy )
		{
			if ( Networking.IsHost )
				SetCurrentWeapon( weapon.Id );

			return;
		}

		weapon.Deploy();
	}

	public void ClearViewModel()
	{
		foreach ( var weapon in Inventory.Weapons )
		{
			weapon.ClearViewModel();
		}
	}

	public void CreateViewModel( bool playDeployEffects = true )
	{
		if ( CameraController.Mode != CameraMode.FirstPerson )
			return;

		var weapon = CurrentWeapon;
		if ( weapon.IsValid() )
			weapon.CreateViewModel( playDeployEffects );
	}
	
	protected float GetEyeHeightOffset()
	{
		if ( IsCrouching ) return -32f;
		if ( HealthComponent.State == LifeState.Dead ) return -48f;
		return 0f;
	}

	protected override void OnStart()
	{
		if ( !IsProxy && !IsBot )
		{
			// Set this as our local player and possess it.
			GameUtils.LocalPlayer = this;
			(this as IPawn).Possess();
		}

		if ( IsBot )
			GameObject.Name += " (Bot)";
	}

	public SceneTraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		var bbox = CharacterController.BoundingBox;
		var mins = bbox.Mins;
		var maxs = bbox.Maxs;

		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		if ( liftHead > 0 )
		{
			end += Vector3.Up * liftHead;
		}

		var tr = Scene.Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithoutTags( "ragdoll", "invis", "pickup" )
					.IgnoreGameObjectHierarchy( GameObject.Root )
					.Run();
		return tr;
	}

	protected override void OnUpdate()
	{
		var cc = CharacterController;
		CurrentHoldType = CurrentWeapon.IsValid() ? CurrentWeapon.GetHoldType() : AnimationHelper.HoldTypes.None;

		// Eye input
		if ( (this as IPawn).IsPossessed && cc.IsValid() )
		{
			var eyeHeightOffset = GetEyeHeightOffset();

			var target = eyeHeightOffset;
			var trace = TraceBBox( Transform.Position, Transform.Position, 0, 10f );
			if ( trace.Hit && target > _smoothEyeHeight )
			{
				// We hit something, that means we can't increase our eye height because something's in the way.
				eyeHeightOffset = _smoothEyeHeight;
				IsCrouching = true;
			}
			else
			{
				eyeHeightOffset = target;
			}

			_smoothEyeHeight = _smoothEyeHeight.LerpTo( eyeHeightOffset, Time.Delta * 10f );
			CharacterController.Height = Height + _smoothEyeHeight;

			if ( PlayerBoxCollider.IsValid() )
			{
				// Bit shit, but it works
				PlayerBoxCollider.Center = new( 0, 0, 32 + _smoothEyeHeight );
				PlayerBoxCollider.Scale = new( 32, 32, 64 + _smoothEyeHeight );
			}

			if ( IsLocallyControlled )
			{
				EyeAngles += Input.AnalogLook * AimDampening;
				EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );
			}

			CameraController.UpdateFromEyes( _smoothEyeHeight );
		}
		else
		{
			CameraController.SetActive( false );
		}

		var rotateDifference = 0f;
		
		if ( Body.IsValid() )
		{
			var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

			rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

			if ( rotateDifference > 50.0f || ( cc != null && cc.Velocity.Length > 10.0f ) )
			{
				Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 10.0f );
			}
		}

		var wasGrounded = IsGrounded;
		IsGrounded = cc.IsOnGround;

		if ( IsGrounded != wasGrounded )
		{
			GroundedChanged( wasGrounded, IsGrounded );
		}

		if ( AnimationHelper.IsValid() )
		{
			AnimationHelper.WithVelocity( cc.Velocity );
			AnimationHelper.WithWishVelocity( WishVelocity );
			AnimationHelper.IsGrounded = IsGrounded;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
			AnimationHelper.MoveStyle = AnimationHelper.MoveStyles.Run;
			AnimationHelper.DuckLevel = IsCrouching ? 100 : 0;
			AnimationHelper.HoldType = CurrentHoldType;
		}

		AimDampening = 1.0f;
	}

	/// <summary>
	/// A network message that lets other users that we've triggered a jump.
	/// </summary>
	[Broadcast]
	public void BroadcastPlayerJumped()
	{
		AnimationHelper?.TriggerJump();
		OnJump?.Invoke();
	}

	private void GroundedChanged( bool wasOnGround, bool isOnGround )
	{
		if ( wasOnGround && !isOnGround )
		{
			JumpPosition = Transform.Position;
		}

		if ( !wasOnGround && isOnGround )
		{
			var vel = PreviousVelocity.z;
			var jumpFromPos = JumpPosition;
			var positionNow = Transform.Position;

			// jumped up somehow?
			if ( positionNow.z >= jumpFromPos.z ) return;

			var fallDamageScale = 125;
			var zDist = MathF.Abs( positionNow.z - jumpFromPos.z );

			var scale = zDist.LerpInverse( 0, 500f, true );
			if ( scale < 0.35f ) return;

			GameObject.TakeDamage( fallDamageScale * scale, Transform.Position, vel, Id, Id );
		}
	}

	/// <summary>
	/// Get the current friction.
	/// </summary>
	/// <returns></returns>
	private float GetFriction()
	{
		if ( !IsGrounded ) return 0.1f;
		if ( IsSlowWalking ) return SlowWalkFriction;
		if ( IsCrouching ) return CrouchingFriction;
		return BaseFriction;
	}

	private void ApplyAcceleration()
	{
		if ( !IsGrounded ) CharacterController.Acceleration = AirAcceleration;
		else if ( IsSlowWalking ) CharacterController.Acceleration = SlowWalkAcceleration;
		else if ( IsCrouching ) CharacterController.Acceleration = CrouchingAcceleration;
		else
			CharacterController.Acceleration = BaseAcceleration;
	}

	private void BuildInput()
	{
		if ( InMenu )
			return;

		IsSlowWalking = Input.Down( "Run" );
		IsCrouching = Input.Down( "Duck" );
		IsUsing = Input.Down( "Use" );
		
		// Check if our current weapon has the planting tag and if so force us to crouch.
		var currentWeapon = CurrentWeapon;
		if ( currentWeapon.IsValid() && currentWeapon.Tags.Has( "planting" ) )
			IsCrouching = true;

		if ( Input.Pressed( "Noclip" ) && Game.IsEditor )
		{
			IsNoclipping = !IsNoclipping;
		}

		if ( WishMove.LengthSquared > 0.01f || Input.Down( "Attack1" ) )
		{
			TimeSinceLastInput = 0f;
		}
	}

	private bool IsOutlineVisible()
	{
		var localPlayer = GameUtils.Viewer;
		if ( !localPlayer.IsValid() )
			return false;

		if ( localPlayer == this )
			return false;

		if ( TeamComponent.Team == Team.Unassigned )
			return false;

		if ( HealthComponent.IsGodMode )
			return true;
		
		return HealthComponent.State == LifeState.Alive && TeamComponent.Team == localPlayer.TeamComponent.Team;
	}

	private void UpdateOutline()
	{
		if ( !IsOutlineVisible() )
		{
			Outline.Enabled = false;
			return;
		}

		Outline.Enabled = true;
		Outline.Width = 0.2f;
		Outline.Color = Color.Transparent;
		Outline.InsideColor = HealthComponent.IsGodMode ? Color.White.WithAlpha( 0.1f ) : Color.Transparent;
		Outline.ObscuredColor = GameUtils.Viewer is { TeamComponent.Team: var localTeam } && localTeam == TeamComponent.Team
			? TeamComponent.Team.GetColor() : Color.Transparent;
	}

	private Vector3 PreviousVelocity;
	Vector3 JumpPosition;

	private void ApplyMovement()
	{
		var cc = CharacterController;

		CheckLadder();

		if ( IsTouchingLadder )
		{
			LadderMove();
			return;
		}

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
		}
		else
		{
			if ( !IsNoclipping )
			{
				cc.Velocity -= Gravity * Time.Delta * 0.5f;
			}
			cc.Accelerate( WishVelocity.ClampLength( 50 ) );
		}
		
		if ( !cc.IsOnGround )
		{
			if ( !IsNoclipping )
			{
				cc.Velocity -= Gravity * Time.Delta * 0.5f;
			}
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}

		if ( IsNoclipping )
		{
			cc.IsOnGround = false;
			cc.Velocity = WishMove.Normal * EyeAngles.ToRotation() * NoclipSpeed;
		}

		cc.ApplyFriction( GetFriction() );
		cc.Move();
	}

	protected override void OnFixedUpdate()
	{
		var cc = CharacterController;
		if ( !cc.IsValid() ) return;

		UpdateZones();
		UpdateOutline();

		if ( HealthComponent.State != LifeState.Alive )
			return;

		if ( Networking.IsHost && IsBot )
		{
			if ( BotFollowHostInput )
			{
				BuildWishInput();
				BuildWishVelocity();
			}

			// If we're a bot call these so they don't float in the air.
			ApplyAcceleration();
			ApplyMovement();
			return;
		}

		if ( !IsLocallyControlled )
			return;

		PreviousVelocity = cc.Velocity;

		UpdateUse();
		UIUpdate();
		BuildWishInput();
		BuildWishVelocity();
		BuildInput();

		if ( cc.IsOnGround && !IsFrozen && !InMenu && Input.Pressed( "Jump" ) )
		{
			cc.Punch( Vector3.Up * JumpPower * 1f );
			BroadcastPlayerJumped();
		}

		ApplyAcceleration();
		ApplyMovement();
	}
	
	private void UIUpdate()
	{
		if ( InBuyMenu )
		{
			if ( Input.EscapePressed || Input.Pressed( "BuyMenu" ) || !CanBuy() )
			{
				InBuyMenu = false;
			}
		}
		else if ( Input.Pressed( "BuyMenu" ) )
		{
			if ( CanBuy() )
			{
				InBuyMenu = true;
			}
		}
	}

	public bool CanBuy()
	{
		if ( GameMode.Instance?.Components.Get<BuyZoneTime>() is { } buyZoneTime )
		{
			return IsInBuyzone() && buyZoneTime.CanBuy();
		}

		return IsInBuyzone();
	}

	public bool IsInBuyzone()
	{
		if ( GameMode.Instance.BuyAnywhere )
			return true;

		var zone = GetZone<BuyZone>();
		if ( zone is null )
			return false;

		if ( zone.Team == Team.Unassigned )
			return true;

		return zone.Team == TeamComponent.Team;
	}

	private float GetWalkSpeed()
	{
		var spd = WalkSpeed;
		var wpn = CurrentWeapon;
		if ( !wpn.IsValid() ) return spd;
		return spd - wpn.SpeedPenalty;
	}

	protected float GetWishSpeed()
	{
		if ( IsSlowWalking ) return 100f;
		if ( IsCrouching ) return 100f;
		return GetWalkSpeed();
	}

	bool IsTouchingLadder = false;
	Vector3 LadderNormal;

	public virtual void CheckLadder()
	{
		var cc = CharacterController;
		var wishvel = new Vector3( WishMove.x.Clamp( -1f, 1f ), WishMove.y.Clamp( -1f, 1f ), 0 );
		wishvel *= EyeAngles.WithPitch( 0 ).ToRotation();
		wishvel = wishvel.Normal;

		if ( IsTouchingLadder )
		{
			if ( Input.Pressed( "jump" ) )
			{
				cc.Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;

			}
			else if ( cc.GroundObject != null && LadderNormal.Dot( wishvel ) > 0 )
			{
				IsTouchingLadder = false;

				return;
			}
		}

		const float ladderDistance = 1.0f;
		var start = Transform.Position;
		Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : wishvel) * ladderDistance;

		var pm = Scene.Trace.Ray( start, end )
					.Size( cc.BoundingBox.Mins, cc.BoundingBox.Maxs )
					.WithTag( "ladder" )
					.HitTriggers()
					.IgnoreGameObjectHierarchy( GameObject )
					.Run();

		// Gizmo.Draw.LineBBox( cc.BoundingBox.Translate( end ) );

		IsTouchingLadder = false;

		if ( pm.Hit )
		{
			IsTouchingLadder = true;
			LadderNormal = pm.Normal;
		}
	}

	public virtual void LadderMove()
	{
		CharacterController.IsOnGround = false;

		var velocity = WishVelocity;
		float normalDot = velocity.Dot( LadderNormal );
		var cross = LadderNormal * normalDot;
		CharacterController.Velocity = (velocity - cross) + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

		CharacterController.Move();
	}

	public void BuildWishInput()
	{
		WishMove = 0f;

		if ( IsFrozen || InMenu )
			return;

		WishMove += Input.AnalogMove;
	}

	public void BuildWishVelocity()
	{
		WishVelocity = 0f;
		
		var rot = EyeAngles.WithPitch( 0f ).ToRotation();

		var wishDirection = WishMove.Normal * rot;
		wishDirection = wishDirection.WithZ( 0 );

		WishVelocity = wishDirection * GetWishSpeed();
	}

	public void AssignTeam( Team team )
	{
		Assert.True( Networking.IsHost );
		TeamComponent.Team = team;

		foreach ( var listener in Scene.GetAllComponents<ITeamAssignedListener>() )
		{
			listener.OnTeamAssigned( this, team );
		}
	}

	/// <summary>
	/// Called when YOU inflict damage on something
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="position"></param>
	/// <param name="force"></param>
	/// <param name="target"></param>
	/// <param name="hitbox"></param>
	void IDamageListener.OnDamageGiven( float damage, Vector3 position, Vector3 force, Component target, string hitbox = "" )
	{
		Log.Info( $"{this} damaged {target} for {damage}" );

		// Did we cause this damage?
		if ( this == GameUtils.Viewer )
		{
			Crosshair.Instance?.Trigger( damage, target, position, hitbox );
		}
	}

	/// <summary>
	/// Called when YOU take damage from something
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="position"></param>
	/// <param name="force"></param>
	/// <param name="attacker"></param>
	/// <param name="hitbox"></param>
	void IDamageListener.OnDamageTaken( float damage, Vector3 position, Vector3 force, Component attacker, string hitbox = "" )
	{
		Log.Info( $"{this} took {damage} damage!" );

		AnimationHelper.ProceduralHitReaction( damage / 100f, force );

		// Is this the local player?
		if ( IsViewer )
		{
			DamageIndicator.Current?.OnHit( position );
		}

		Body.DamageTakenPosition = position;
		Body.DamageTakenForce = force.Normal * damage * Game.Random.Float(5f, 20f);

		// Headshot effects
		if ( hitbox.Contains( "head" ) )
		{
			var hasHelmet = HealthComponent.HasHelmet;

			// Non-local viewer
			if ( !IsViewer )
			{
				var go = hasHelmet ? HeadshotWithHelmetEffect?.Clone( position ) : HeadshotEffect?.Clone( position );

				if ( go.IsValid() )
				{
					// we did it
				}
			}

			var headshotSound = hasHelmet ? HeadshotWithHelmetSound : HeadshotSound;
			if ( headshotSound is not null )
			{
				Sound.Play( headshotSound, position );
			}
		}

		if ( BloodEffect.IsValid() )
		{
			BloodEffect?.Clone( new CloneConfig()
			{
				StartEnabled = true,
				Transform = new( position ),
				Name = $"Blood effect from ({GameObject})"
			} );
		}

		if ( BloodImpactSound is not null )
		{
			var snd = Sound.Play( BloodImpactSound, position );
			snd.ListenLocal = IsViewer;
		}
	}
}
