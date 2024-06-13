using Sandbox.Events;

namespace Facepunch;

public partial class PlayerController :
	IGameEventHandler<WeaponDeployedEvent>,
	IGameEventHandler<WeaponHolsteredEvent>
{
	/// <summary>
	/// What weapon are we using?
	/// </summary>
	[Property, ReadOnly] public Weapon CurrentWeapon { get; private set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold our ViewModel.
	/// </summary>
	[Property] public GameObject ViewModelGameObject { get; set; }

	/// <summary>
	/// How inaccurate are things like gunshots?
	/// </summary>
	public float Spread { get; set; }

	private void UpdateRecoilAndSpread()
	{
		bool isAiming = CurrentWeapon?.Tags.Has( "aiming" ) ?? false;
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

	void IGameEventHandler<WeaponDeployedEvent>.OnGameEvent( WeaponDeployedEvent eventArgs )
	{
		CurrentWeapon = eventArgs.Weapon;
	}

	void IGameEventHandler<WeaponHolsteredEvent>.OnGameEvent( WeaponHolsteredEvent eventArgs )
	{
		if ( eventArgs.Weapon == CurrentWeapon )
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

	public TimeSince TimeSinceWeaponDeployed { get; private set; }

	public void SetCurrentWeapon( Weapon weapon )
	{
		if ( IsProxy )
		{
			if ( Networking.IsHost )
				SetCurrentWeapon( weapon.Id );

			return;
		}

		TimeSinceWeaponDeployed = 0;

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
}
