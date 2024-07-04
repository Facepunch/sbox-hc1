namespace Facepunch;

[Title( "2D Scope" ), Group( "Weapon Components" )]
public class ScopeWeaponComponent : InputWeaponComponent
{
	[Property] public Material ScopeOverlay { get; set; }
	[Property] public SoundEvent ZoomSound { get; set; }
	[Property] public SoundEvent UnzoomSound { get; set; }

	IDisposable renderHook;

	private int ZoomLevel { get; set; } = 0;
	public bool IsZooming => ZoomLevel > 0;
	private float BlurLerp { get; set; } = 1.0f;

	private Angles LastAngles;
	private Angles AnglesLerp;
	[Property] private float AngleOffsetScale { get; set; } = 0.01f;
	[Property] public List<int> ZoomLevels { get; set; } = new();

	protected void StartZoom( int level = 0 )
	{
		renderHook?.Dispose();
		renderHook = null;

		if ( !Equipment.IsValid() )
			return;

		if ( !Equipment.Owner.IsValid() )
			return;

		var camera = Equipment.Owner.CameraController;

		if ( ScopeOverlay is not null )
			renderHook = camera.Camera.AddHookAfterTransparent( "Scope", 100, RenderEffect );

		if ( ZoomSound is not null )
			Sound.Play( ZoomSound, Equipment.GameObject.Transform.Position );

		ZoomLevel = level;
		Equipment.Tags.Add( "aiming" );

		if ( Equipment.ViewModel.IsValid() )
		{
			Equipment.ViewModel.GameObject.Enabled = false;
		}
	}

	protected void EndZoom()
	{
		if ( renderHook is not null )
			renderHook.Dispose();

		if ( UnzoomSound is not null && Equipment.IsValid() )
			Sound.Play( UnzoomSound, Equipment.GameObject.Transform.Position );

		ZoomLevel = 0;

		if ( Equipment.IsValid() )
		{
			Equipment.Tags.Remove( "aiming" );

			if ( Equipment.ViewModel.IsValid() )
			{
				Equipment.ViewModel.GameObject.Enabled = true;
			}
		}

		AnglesLerp = new Angles();
		BlurLerp = 1.0f;
	}

	public void RenderEffect( SceneCamera camera )
	{
		RenderAttributes attrs = new RenderAttributes();

		attrs.Set( "BlurAmount", BlurLerp );
		attrs.Set( "Offset", new Vector2( AnglesLerp.yaw, -AnglesLerp.pitch ) * AngleOffsetScale );

		Graphics.Blit( ScopeOverlay, attrs );
	}

	protected override void OnInputDown()
	{
		if ( ZoomLevel < ZoomLevels.Count )
		{
			StartZoom( ZoomLevel + 1 );
		}
		else
		{
			EndZoom();
		}
	}

	protected virtual bool CanAim()
	{
		if ( Tags.Has( "reloading" ) ) return false;

		return true;
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		EndZoom();
	}

	protected override void OnParentChanged( GameObject oldParent, GameObject newParent )
	{
		base.OnParentChanged( oldParent, newParent );
		EndZoom();
	}

	public float GetFOV()
	{
		if ( ZoomLevel < 1 ) return 0f;
		return ZoomLevels[Math.Clamp( ZoomLevel - 1, 0, ZoomLevels.Count )];
	}

	protected override void OnUpdate()
	{
		if ( !IsZooming )
			return;

		var camera = Equipment?.Owner?.CameraController;
		if ( !camera.IsValid() )
			return;

		if ( !CanAim() )
		{
			EndZoom();
		}

		if ( Equipment.Owner.CurrentEquipment != Equipment )
		{
			EndZoom();
		}

		Equipment.Owner.AimDampening /= (ZoomLevel * ZoomLevel) + 1;

		{
			var cc = Equipment.Owner.CharacterController;

			float velocity = Equipment.Owner.CharacterController.Velocity.Length / 25.0f;
			float blur = 1.0f / (velocity + 1.0f);
			blur = MathX.Clamp( blur, 0.1f, 1.0f );

			if ( !cc.IsOnGround )
				blur = 0.1f;

			if ( blur > BlurLerp )
				BlurLerp = BlurLerp.LerpTo( blur, Time.Delta * 1.0f );
			else
				BlurLerp = BlurLerp.LerpTo( blur, Time.Delta * 10.0f );

			var angles = Equipment.Owner.EyeAngles;
			var delta = angles - LastAngles;

			AnglesLerp = AnglesLerp.LerpTo( delta, Time.Delta * 10.0f );
			LastAngles = angles;
		}

	}
};
