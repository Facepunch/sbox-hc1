using Sandbox.Events;

namespace Facepunch;

public sealed partial class PlayerController : Component, IPawn, IRespawnable
{
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
	/// The spotter for this player.
	/// </summary>
	[RequireComponent] public Spotter Spotter { get; set; }

	/// <summary>
	/// The spottable for this player.
	/// </summary>
	[RequireComponent] public Spottable Spottable { get; set; }

	/// <summary>
	/// Unique Ids of this player
	/// </summary>
	[RequireComponent] public PlayerId PlayerId { get; set; }

	/// <summary>
	/// A reference to the View Model's camera. This will be disabled by the View Model.
	/// </summary>
	[Property] public CameraComponent ViewModelCamera { get; set; }

	/// <summary>
	/// Handles the player's outfit
	/// </summary>
	[Property] public PlayerOutfitter Outfitter { get; set; }

	/// <summary>
	/// Get a quick reference to the real Camera GameObject.
	/// </summary>
	public GameObject CameraGameObject => CameraController.Camera.GameObject;

	/// <summary>
	/// Finds the first <see cref="SkinnedModelRenderer"/> on <see cref="Body"/>
	/// </summary>
	public SkinnedModelRenderer BodyRenderer => Body.Components.Get<SkinnedModelRenderer>();

	/// <summary>
	/// IPawn
	/// </summary>
	Team IPawn.Team => TeamComponent.Team;

	/// <summary>
	/// IPawn
	/// </summary>
	CameraComponent IPawn.Camera => CameraController.Camera;

	public bool InBuyMenu { get; private set; }
	public bool InMenu => InBuyMenu;
	
	protected override void OnStart()
	{
		if ( !IsProxy && !IsBot )
		{
			// Set this as our local player and possess it.
			GameUtils.LocalPlayer = this;
			(this as IPawn).Possess();
		}

		// TODO: expose these parameters please
		TagBinder.BindTag( "no_shooting", () => IsSprinting || TimeSinceSprintChanged < 0.25f || TimeSinceWeaponDeployed < 0.66f );
		TagBinder.BindTag( "no_aiming", () => IsSprinting || TimeSinceSprintChanged < 0.25f || TimeSinceGroundedChanged < 0.25f );

		if ( IsBot )
			GameObject.Name += " (Bot)";
	}

	protected override void OnUpdate()
	{
		OnUpdateMovement();

		CrouchAmount = CrouchAmount.LerpTo( IsCrouching ? 1 : 0, Time.Delta * CrouchLerpSpeed() );
		_smoothEyeHeight = _smoothEyeHeight.LerpTo( _eyeHeightOffset * (IsCrouching ? CrouchAmount : 1), Time.Delta * 10f );
		CharacterController.Height = Height + _smoothEyeHeight;

		if ( IsLocallyControlled )
		{
			DebugUpdate();
		}
	}

	protected override void OnFixedUpdate()
	{
		var cc = CharacterController;
		if ( !cc.IsValid() ) return;

		UpdateEyes();
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

		_previousVelocity = cc.Velocity;

		UpdateUse();
		UIUpdate();
		BuildWishInput();
		BuildWishVelocity();
		BuildInput();

		UpdateRecoilAndSpread();
		ApplyAcceleration();
		ApplyMovement();
	}
	
	// I don't think this belongs here either. Should just be able to do this in the UI system.
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

	// Can we do this differently? I don't like it.
	public bool CanBuy()
	{
		if ( GameMode.Instance?.Components.Get<BuyZoneTime>() is { } buyZoneTime )
		{
			return IsInBuyzone() && buyZoneTime.CanBuy();
		}

		return IsInBuyzone();
	}

	// Same with this, I don't like it.
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

	public void AssignTeam( Team team )
	{
		if ( !Networking.IsHost )
			return;

		TeamComponent.Team = team;

		Scene.Dispatch( new TeamAssignedEvent( this, team ) );
	}
}
