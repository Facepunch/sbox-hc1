namespace Facepunch;

/// <summary>
/// A weapon component.
/// </summary>
public partial class Weapon : Component, Component.INetworkListener, IWeapon
{
	/// <summary>
	/// Subscribe to changes in the weapon's deployed state.
	/// </summary>
	public interface IDeploymentListener
	{
		public void OnDeployed( Weapon weapon ) { }
		public void OnHolstered( Weapon weapon ) { }
	}
	
	/// <summary>
	/// A reference to the weapon's <see cref="WeaponData"/>.
	/// </summary>
	[Property] public WeaponData Resource { get; set; }

	/// <summary>
	/// A tag binder for this weapon.
	/// </summary>
	[RequireComponent] public TagBinder TagBinder { get; set; }

	/// <summary>
	/// Shorthand to bind a tag.
	/// </summary>
	/// <param name="tag"></param>
	/// <param name="predicate"></param>
	internal void BindTag( string tag, Func<bool> predicate ) => TagBinder.BindTag( tag, predicate );

	/// <summary>
	/// A reference to the weapon's model renderer.
	/// </summary>
	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }

	/// <summary>
	/// The default holdtype for this weapon.
	/// </summary>
	[Property] protected AnimationHelper.HoldTypes HoldType { get; set; } = AnimationHelper.HoldTypes.Rifle;

	/// <summary>
	/// What sound should we play when taking this gun out?
	/// </summary>
	[Property, Group( "Sounds" )] public SoundEvent DeploySound { get; set; }

	/// <summary>
	/// How slower do we walk with this weapon out?
	/// </summary>
	[Property, Group( "Movement" )] public float SpeedPenalty { get; set; } = 0f;

	[Property, Group( "GameObjects" )] public GameObject Muzzle { get; set; }
	[Property, Group( "GameObjects" )] public GameObject EjectionPort { get; set; }

	/// <summary>
	/// Is this weapon currently deployed by the player?
	/// </summary>
	[Sync, Change( nameof( OnIsDeployedPropertyChanged ))]
	public bool IsDeployed { get; private set; }
	private bool _wasDeployed { get; set; }
	private bool _hasStarted { get; set; }

	[DeveloperCommand( "Toggle View Model" )]
	private static void ToggleViewModel()
	{
		var player = GameUtils.Viewer;

		player.CurrentWeapon.ViewModel.ModelRenderer.Enabled = !player.CurrentWeapon.ViewModel.ModelRenderer.Enabled;
		player.CurrentWeapon.ViewModel.Arms.Enabled = !player.CurrentWeapon.ViewModel.Arms.Enabled;
	}

	/// <summary>
	/// Updates the render mode, if we're locally controlling a player, we want to hide the world model.
	/// </summary>
	protected void UpdateRenderMode()
	{
		if ( PlayerController.IsViewer )
			ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.ShadowsOnly;
		else
			ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;
	}

	private ViewModel viewModel;

	/// <summary>
	/// A reference to the weapon's <see cref="Facepunch.ViewModel"/> if it has one.
	/// </summary>
	public ViewModel ViewModel
	{
		get => viewModel;
		set
		{
			viewModel = value;

			if ( viewModel.IsValid() )
			{
				viewModel.Weapon = this;
				viewModel.ViewModelCamera = PlayerController.ViewModelCamera;
			}
		}
	}

	/// <summary>
	/// Get the weapon's owner - namely the player controller
	/// </summary>
	public PlayerController PlayerController => Components.Get<PlayerController>( FindMode.EverythingInAncestors );

	/// <summary>
	/// How long it's been since we used this attack.
	/// </summary>
	protected TimeSince TimeSincePrimaryAttack { get; set; }

	/// <summary>
	/// How long it's been since we used this attack.
	/// </summary>
	protected TimeSince TimeSinceSecondaryAttack { get; set; }
	
	void INetworkListener.OnDisconnected( Connection connection )
	{
		if ( !Networking.IsHost )
			return;

		if ( !Resource.DropOnDisconnect )
			return;
		
		var player = GameUtils.AllPlayers.FirstOrDefault( x => x.Network.OwnerConnection == connection );
		if ( !player.IsValid() ) return;
		
		var droppedWeapon = DroppedWeapon.Create( Resource, player.Transform.Position + Vector3.Up * 32f, Rotation.Identity, this );
		droppedWeapon.GameObject.NetworkSpawn();
	}

	/// <summary>
	/// Deploy this weapon.
	/// </summary>
	[Authority]
	public void Deploy()
	{
		if ( IsDeployed )
			return;
		
		// We must first holster all other weapons.
		if ( PlayerController.IsValid() )
		{
			var weapons = PlayerController.Inventory.Weapons.ToList();

			foreach ( var weapon in weapons )
				weapon.Holster();
		}
		
		IsDeployed = true;
	}
	
	/// <summary>
	/// Holster this weapon.
	/// </summary>
	[Authority]
	public void Holster()
	{
		if ( !IsDeployed )
			return;
		
		IsDeployed = false;
	}

	/// <summary>
	/// Allow weapons to override holdtypes at any notice.
	/// </summary>
	/// <returns></returns>
	public virtual AnimationHelper.HoldTypes GetHoldType()
	{
		return HoldType;
	}

	/// <summary>
	/// Called when trying to shoot the weapon with the "Attack1" input action.
	/// </summary>
	/// <returns></returns>
	public virtual bool PrimaryAttack()
	{
		return true;
	}

	/// <summary>
	/// Can we even use this attack?
	/// </summary>
	/// <returns></returns>
	public virtual bool CanPrimaryAttack()
	{
		return TimeSincePrimaryAttack > 0.1f;
	}

	/// <summary>
	/// Called when trying to shoot the weapon with the "Attack2" input action.
	/// </summary>
	/// <returns></returns>
	public virtual bool SecondaryAttack()
	{
		return false;
	}

	/// <summary>
	/// Can we even use this attack?
	/// </summary>
	/// <returns></returns>
	public virtual bool CanSecondaryAttack()
	{
		return false;
	}
	
	private void OnIsDeployedPropertyChanged( bool oldValue, bool newValue )
	{
		// Conna: If `OnStart` hasn't been called yet, don't do anything. It'd be nice to have a property on
		// a Component that can indicate this.
		if ( !_hasStarted ) return;
		UpdateDeployedState();
	}

	private void UpdateDeployedState()
	{
		if ( IsDeployed == _wasDeployed )
			return;

		switch ( _wasDeployed )
		{
			case false when IsDeployed:
				OnDeployed();
				break;
			case true when !IsDeployed:
				OnHolstered();
				break;
		}

		_wasDeployed = IsDeployed;
	}

	public void ClearViewModel()
	{
		if ( ViewModel.IsValid() )
			ViewModel.GameObject.Destroy();
	}
	
	/// <summary>
	/// Creates a viewmodel for the player to use.
	/// </summary>
	public void CreateViewModel( bool playDeployEffects = true )
	{
		var player = PlayerController;
		if ( !player.IsValid() ) return;
		
		var resource = Resource;

		ClearViewModel();
		UpdateRenderMode();

		if ( resource.ViewModelPrefab.IsValid() )
		{
			// Create the weapon prefab and put it on the weapon gameobject.
			var viewModelGameObject = resource.ViewModelPrefab.Clone( new CloneConfig()
			{
				Transform = new(),
				Parent = player.ViewModelGameObject,
				StartEnabled = true,
			} );

			var viewModelComponent = viewModelGameObject.Components.Get<ViewModel>();

			// Weapon needs to know about the ViewModel
			ViewModel = viewModelComponent;

			viewModelGameObject.BreakFromPrefab();
		}

		if ( !playDeployEffects )
			return;

		if ( DeploySound is null )
			return;

		var snd = Sound.Play( DeploySound, Transform.Position );
		if ( !snd.IsValid() ) return;
		
		snd.ListenLocal = !IsProxy;
	}

	protected override void OnStart()
	{
		_wasDeployed = IsDeployed;
		_hasStarted = true;
		
		if ( IsDeployed )
			OnDeployed();
		else
			OnHolstered();
		
		base.OnStart();
	}
	
	protected virtual void OnDeployed()
	{
		if ( PlayerController.IsValid() && PlayerController.IsViewer && PlayerController.CameraController.Mode != CameraMode.ThirdPerson )
			CreateViewModel();
		
		if ( ModelRenderer.IsValid() )
			ModelRenderer.Enabled = true;
		
		var listeners = GameObject.Root.Components.GetAll<IDeploymentListener>();
		foreach ( var listener in listeners )
			listener.OnDeployed( this );
	}

	protected virtual void OnHolstered()
	{
		if ( ModelRenderer.IsValid() )
			ModelRenderer.Enabled = false;
		
		ClearViewModel();

		var listeners = GameObject.Root.Components.GetAll<IDeploymentListener>();
		foreach ( var listener in listeners )
			listener.OnHolstered( this );
	}

	protected override void OnDestroy()
	{
		ClearViewModel();
	}
}
