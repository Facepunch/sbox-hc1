namespace Facepunch;

/// <summary>
/// A weapon component.
/// </summary>
public partial class Weapon : Component
{
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
	/// Is this weapon currently deployed by the player?
	/// </summary>
	[Sync] public bool IsDeployed { get; set; }
	private bool _wasDeployed { get; set; }

	/// <summary>
	/// Updates the render mode, if we're locally controlling a player, we want to hide the world model.
	/// </summary>
	protected void UpdateRenderMode()
	{
		if ( PlayerController.IsLocallyControlled )
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
	/// Access a function.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T GetFunction<T>() where T : WeaponFunction
	{
		return Components.Get<T>( FindMode.EnabledInSelfAndChildren );
	}

	/// <summary>
	/// Access any amount of functions.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public IEnumerable<T> GetFunctions<T>() where T : WeaponFunction
	{
		return Components.GetAll<T>( FindMode.EnabledInSelfAndChildren );
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

	protected override void OnUpdate()
	{
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
		if ( !ViewModel.IsValid() )
			return;

		ViewModel.GameObject.Destroy();
		ViewModel = null;
	}
	
	/// <summary>
	/// Creates a viewmodel for the player to use.
	/// </summary>
	public void CreateViewModel()
	{
		var player = PlayerController;
		if ( !player.IsValid() ) return;
		
		var resource = Resource;

		ClearViewModel();
		UpdateRenderMode();

		if ( resource.ViewModelPrefab != null )
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
		}

		if ( DeploySound is null )
			return;

		var snd = Sound.Play( DeploySound, Transform.Position );
		snd.ListenLocal = !IsProxy;
	}

	protected override void OnStart()
	{
		_wasDeployed = IsDeployed;
		
		if ( IsDeployed )
			OnDeployed();
		else
			OnHolstered();
		
		base.OnStart();
	}
	
	protected virtual void OnDeployed()
	{
		if ( !IsProxy )
			CreateViewModel();
		
		ModelRenderer.Enabled = true;
	}

	protected virtual void OnHolstered()
	{
		ModelRenderer.Enabled = false;
		ClearViewModel();
	}

	protected override void OnDestroy()
	{
		if ( ViewModel.IsValid() )
			ViewModel.GameObject.Destroy();
	}
}
