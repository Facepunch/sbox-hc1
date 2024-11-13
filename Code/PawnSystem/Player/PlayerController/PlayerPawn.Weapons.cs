using Sandbox.Events;

namespace Facepunch;

public partial class PlayerPawn :
	IGameEventHandler<EquipmentDeployedEvent>,
	IGameEventHandler<EquipmentHolsteredEvent>
{
	/// <summary>
	/// What weapon are we using?
	/// </summary>
	[Property, ReadOnly] public Equipment CurrentEquipment { get; private set; }

	public GameObject ViewModelGameObject => CameraController.CameraObject;

	/// <summary>
	/// How inaccurate are things like gunshots?
	/// </summary>
	public float Spread { get; set; }

	/// <summary>
	/// Can we open and use the buy menu?
	/// </summary>
	public bool CanBuy => HealthComponent.State == LifeState.Alive && IsInBuyZone;

	private bool IsInBuyZone => PlayerState?.BuyMenuMode is BuyMenuMode.EnabledEverywhere
		|| PlayerState?.BuyMenuMode is BuyMenuMode.EnabledInBuyZone && GetZone<BuyZone>() is not null;

	private void UpdateRecoilAndSpread()
	{
		bool isAiming = CurrentEquipment.IsValid() && CurrentEquipment.Tags.Has( "aiming" );

		var spread = Global.BaseSpreadAmount;
		var scale = Global.VelocitySpreadScale;
		if ( isAiming ) spread *= Global.AimSpread;
		if ( isAiming ) scale *= Global.AimVelocitySpreadScale;

		var velLen = CharacterController.Velocity.Length;
		spread += velLen.Remap( 0, Global.SpreadVelocityLimit, 0, 1, true ) * scale;

		if ( IsCrouching && IsGrounded ) spread *= Global.CrouchSpreadScale;
		if ( !IsGrounded ) spread *= Global.AirSpreadScale;

		Spread = spread;
	}

	void IGameEventHandler<EquipmentDeployedEvent>.OnGameEvent( EquipmentDeployedEvent eventArgs )
	{
		CurrentEquipment = eventArgs.Equipment;
	}

	void IGameEventHandler<EquipmentHolsteredEvent>.OnGameEvent( EquipmentHolsteredEvent eventArgs )
	{
		if ( eventArgs.Equipment == CurrentEquipment )
			CurrentEquipment = null;
	}

	[Authority]
	private void SetCurrentWeapon( Equipment equipment )
	{
		SetCurrentEquipment( equipment );
	}

	[Authority]
	private void ClearCurrentWeapon()
	{
		if ( CurrentEquipment.IsValid() ) CurrentEquipment.Holster();
	}

	public void Holster()
	{
		if ( IsProxy )
		{
			if ( Networking.IsHost )
				ClearCurrentWeapon();

			return;
		}

		CurrentEquipment?.Holster();
	}

	public TimeSince TimeSinceWeaponDeployed { get; private set; }

	public void SetCurrentEquipment( Equipment weapon )
	{
		if ( weapon == CurrentEquipment ) 
			return;

		ClearCurrentWeapon();

		if ( IsProxy )
		{
			if ( Networking.IsHost )
				SetCurrentWeapon( weapon );

			return;
		}

		TimeSinceWeaponDeployed = 0;

		weapon.Deploy();
	}

	public void ClearViewModel()
	{
		foreach ( var weapon in Inventory.Equipment )
		{
			weapon.ClearViewModel();
		}
	}

	public void CreateViewModel( bool playDeployEffects = true )
	{
		if ( CameraController.Mode != CameraMode.FirstPerson )
			return;

		var weapon = CurrentEquipment;
		if ( weapon.IsValid() )
			weapon.CreateViewModel( playDeployEffects );
	}
}
