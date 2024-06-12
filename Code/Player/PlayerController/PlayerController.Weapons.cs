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
	/// How much spread should we add based on how fast the player is moving
	/// </summary>
	[Property, Group( "Spread" )] public float VelocitySpreadScale { get; set; } = 0.1f;
	[Property, Group( "Spread" )] public float AimSpreadScale { get; set; } = 0.5f;
	[Property, Group( "Spread" )] public float CrouchSpreadScale { get; set; } = 0.5f;
	[Property, Group( "Spread" )] public float AirSpreadScale { get; set; } = 2.0f;
	[Property, Group( "Spread" )] public float SpreadVelocityLimit { get; set; } = 350f;

	/// <summary>
	/// How inaccurate are things like gunshots?
	/// </summary>
	public float Spread { get; set; }

	private void UpdateRecoilAndSpread()
	{
		var spread = 0f;
		var scale = VelocitySpreadScale;
		if ( CurrentWeapon?.Tags.Has( "aiming" ) ?? false ) scale *= AimSpreadScale;

		var velLen = CharacterController.Velocity.Length;
		spread += velLen.Remap( 0, SpreadVelocityLimit, 0, 1, true ) * scale;

		if ( IsCrouching && IsGrounded ) spread *= CrouchSpreadScale;
		if ( !IsGrounded ) spread *= AirSpreadScale;

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
}
