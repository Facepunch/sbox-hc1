using Sandbox.Events;

namespace Facepunch;

public record EquipmentDeployedEvent( Equipment Equipment ) : IGameEvent;
public record EquipmentHolsteredEvent( Equipment Equipment ) : IGameEvent;
public record EquipmentDestroyedEvent( Equipment Equipment ) : IGameEvent;
public record EquipmentTagChanged( Equipment Equipment, string Tag, bool Value ) : IGameEvent;

/// <summary>
/// An equipment component.
/// </summary>
public partial class Equipment : Component, Component.INetworkListener, IDescription
{
	/// <summary>
	/// A reference to the equipment's <see cref="EquipmentResource"/>.
	/// </summary>
	[Property, Group( "Resources" )] public EquipmentResource Resource { get; set; }

	/// <summary>
	/// A tag binder for this equipment.
	/// </summary>
	[RequireComponent] public TagBinder TagBinder { get; set; }

	/// <summary>
	/// Shorthand to bind a tag.
	/// </summary>
	/// <param name="tag"></param>
	/// <param name="predicate"></param>
	internal void BindTag( string tag, Func<bool> predicate ) => TagBinder.BindTag( tag, predicate );

	/// <summary>
	/// The default holdtype for this equipment.
	/// </summary>
	[Property, Group( "Animation" )] protected AnimationHelper.HoldTypes HoldType { get; set; } = AnimationHelper.HoldTypes.Rifle;

	/// <summary>
	/// The default holdtype for this equipment.
	/// </summary>
	[Property, Group( "Animation" )] public AnimationHelper.Hand Handedness { get; set; } = AnimationHelper.Hand.Right;

	/// <summary>
	/// What sound should we play when taking this gun out?
	/// </summary>
	[Property, Group( "Sounds" )] public SoundEvent DeploySound { get; set; }

	/// <summary>
	/// How slower do we walk with this equipment out?
	/// </summary>
	[Property, Group( "Movement" )] public float SpeedPenalty { get; set; } = 0f;

	/// <summary>
	/// What prefab should we spawn as the mounted version of this piece of equipment?
	/// </summary>
	[Property, Group( "Mount Points" )] public GameObject MountedPrefab { get; set; }

	/// <summary>
	/// Should we enable the crosshair?
	/// </summary>
	[Property, Group( "UI" )] public bool UseCrosshair { get; set; } = true;

	/// <summary>
	/// What type of crosshair do we wanna use
	/// </summary>
	[Property, Group( "UI" )] public CrosshairType CrosshairType { get; set; } = CrosshairType.Default;

	/// <summary>
	/// The <see cref="PlayerPawn"/> who owns this.
	/// </summary>
	[Sync( SyncFlags.FromHost )] public PlayerPawn Owner { get; set; }

	/// <summary>
	/// What flags do we have?
	/// </summary>
	[Sync] protected NetList<string> EquipmentTags { get; set; } = new();

	public bool HasTag( string tag )
	{
		return EquipmentTags.Contains( tag );
	}

	public void SetTag( string tag, bool value = true )
	{
		if ( value && !HasTag( tag ) ) EquipmentTags.Add( tag );
		if ( !value && HasTag( tag ) ) EquipmentTags.Remove( tag );

		GameObject.Dispatch( new EquipmentTagChanged( this, tag, value ) );
	}

	public void ToggleTag( string tag )
	{
		SetTag( tag, !HasTag( tag ) );
	}

	// IDescription
	string IDescription.DisplayName => Resource.Name;
	// string IDescription.Icon => Resource.Icon;

	/// <summary>
	/// Is this equipment currently deployed by the player?
	/// </summary>
	[Sync, Change( nameof( OnIsDeployedPropertyChanged ) )]
	public bool IsDeployed { get; private set; }
	private bool _wasDeployed { get; set; }
	private bool _hasStarted { get; set; }

	[DeveloperCommand( "Toggle View Model", "Visuals" )]
	private static void ToggleViewModel()
	{
		var player = Client.Viewer.PlayerPawn;

		player.CurrentEquipment.ViewModel.ModelRenderer.Enabled = !player.CurrentEquipment.ViewModel.ModelRenderer.Enabled;
		player.CurrentEquipment.ViewModel.Arms.Enabled = !player.CurrentEquipment.ViewModel.Arms.Enabled;
	}

	/// <summary>
	/// Updates the render mode, if we're locally controlling a player, we want to hide the world model.
	/// </summary>
	public void UpdateRenderMode( bool force = false )
	{
		if ( WorldModel.IsValid() )
		{
			WorldModel.Enabled = IsDeployed || force;
		}
	}

	private ViewModel viewModel;

	/// <summary>
	/// A reference to the equipment's <see cref="Facepunch.ViewModel"/> if it has one.
	/// </summary>
	public ViewModel ViewModel
	{
		get => viewModel;
		set
		{
			viewModel = value;

			if ( viewModel.IsValid() )
			{
				viewModel.Equipment = this;
			}
		}
	}

	void INetworkListener.OnDisconnected( Connection connection )
	{
		if ( !Networking.IsHost )
			return;

		if ( !Resource.DropOnDisconnect )
			return;

		var player = GameUtils.PlayerPawns.FirstOrDefault( x => x.Network.Owner == connection );
		if ( !player.IsValid() ) return;

		DroppedEquipment.Create( Resource, player.WorldPosition + Vector3.Up * 32f, Rotation.Identity, this );
	}

	/// <summary>
	/// Deploy this equipment.
	/// </summary>
	[Rpc.Owner]
	public void Deploy()
	{
		if ( IsDeployed )
			return;

		// We must first holster all other equipment items.
		if ( Owner.IsValid() )
		{
			var equipment = Owner.Inventory.Equipment.ToList();

			foreach ( var item in equipment )
				item.Holster();
		}

		IsDeployed = true;
	}

	/// <summary>
	/// Holster this equipment.
	/// </summary>
	[Rpc.Owner]
	public void Holster()
	{
		if ( !IsDeployed )
			return;

		IsDeployed = false;
	}

	/// <summary>
	/// Allow equipment to override holdtypes at any notice.
	/// </summary>
	/// <returns></returns>
	public virtual AnimationHelper.HoldTypes GetHoldType()
	{
		return HoldType;
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

	public void DestroyViewModel()
	{
		if ( ViewModel.IsValid() )
			ViewModel.GameObject.Destroy();
	}

	public WorldModel WorldModel { get; set; }

	protected void CreateWorldModel()
	{
		DestroyWorldModel();

		if ( Resource.WorldModelPrefab is null )
		{
			return;
		}

		var parentBone = Owner.HoldGameObject;
		var wm = Resource.WorldModelPrefab.Clone( new CloneConfig { Parent = parentBone, StartEnabled = false, Transform = global::Transform.Zero } );

		wm.Flags |= GameObjectFlags.NotSaved | GameObjectFlags.NotNetworked;
		wm.Enabled = true;

		WorldModel = wm.GetComponent<WorldModel>();
	}

	protected void DestroyWorldModel()
	{
		WorldModel?.DestroyGameObject();
		WorldModel = null;
	}

	/// <summary>
	/// Creates a viewmodel for the player to use.
	/// </summary>
	public void CreateViewModel( bool playDeployEffects = true )
	{
		var player = Owner;
		if ( !player.IsValid() ) return;

		var resource = Resource;

		DestroyViewModel();
		UpdateRenderMode();

		if ( resource.ViewModelPrefab.IsValid() )
		{
			// Create the equipment prefab and put it on the equipment gameobject.
			var viewModelGameObject = resource.ViewModelPrefab.Clone( new CloneConfig()
			{
				Transform = new(),
				Parent = GameObject,
				StartEnabled = true,
			} );

			viewModelGameObject.Flags |= GameObjectFlags.Absolute;

			var viewModelComponent = viewModelGameObject.GetComponent<ViewModel>();
			viewModelComponent.PlayDeployEffects = playDeployEffects;

			// equipment needs to know about the ViewModel
			ViewModel = viewModelComponent;

			viewModelGameObject.BreakFromPrefab();
		}

		if ( !playDeployEffects )
			return;

		if ( DeploySound is null )
			return;

		var snd = Sound.Play( DeploySound, WorldPosition );
		if ( !snd.IsValid() ) return;

		snd.SpacialBlend = Owner.IsViewer ? 0 : snd.SpacialBlend;
	}

	protected override void OnStart()
	{
		_wasDeployed = IsDeployed;
		_hasStarted = true;

		if ( IsDeployed )
			OnDeployed();
		else
			OnHolstered();
	}

	[Sync] bool HasCreatedViewModel { get; set; } = false;


	[Property, Group( "Extras" )]
	public float AimFovOffset { get; set; } = -5;

	protected virtual void OnDeployed()
	{
		if ( Owner.IsValid() && Owner.IsViewer && Owner.CameraController.Mode == CameraMode.FirstPerson )
			CreateViewModel( !HasCreatedViewModel );

		if ( !IsProxy )
			HasCreatedViewModel = true;

		UpdateRenderMode();

		CreateWorldModel();

		GameObject.Root.Dispatch( new EquipmentDeployedEvent( this ) );
	}

	protected virtual void OnHolstered()
	{
		UpdateRenderMode();
		DestroyWorldModel();
		DestroyViewModel();

		HasCreatedViewModel = false;

		GameObject.Root.Dispatch( new EquipmentHolsteredEvent( this ) );
	}

	protected override void OnDestroy()
	{
		DestroyViewModel();

		GameObject.Root.Dispatch( new EquipmentDestroyedEvent( this ) );
	}
}
