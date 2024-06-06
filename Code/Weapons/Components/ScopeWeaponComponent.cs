using Sandbox;

namespace Facepunch;

public class ScopeWeaponComponent : InputWeaponComponent
{
	[Property] public Material ScopeOverlay { get; set; }
	[Property] public SoundEvent ZoomSound { get; set; }
	[Property] public SoundEvent UnzoomSound { get; set;}
	
	IDisposable renderHook;

	[Sync, Change( nameof( OnZoomChanged ))]
	private int ZoomLevel { get; set; } = 0;
	public bool IsZooming => ZoomLevel > 0;
	private float BlurLerp { get; set; } = 1.0f;

	private Angles LastAngles;
	private Angles AnglesLerp;
	[Property] private float AngleOffsetScale { get; set; } = 0.01f;
	[Property] public List<int> ZoomLevels { get; set; } = new();

	protected void StartZoom()
	{
		renderHook?.Dispose();
		renderHook = null;

		if ( !Weapon.IsValid() )
			return;

		if ( !Weapon.PlayerController.IsValid() )
			return;

		var camera = Weapon.PlayerController.CameraController;

		if ( ScopeOverlay is not null )
			renderHook = camera.Camera.AddHookAfterTransparent( "Scope", 100, RenderEffect );

		if( ZoomSound is not null )
			Sound.Play( ZoomSound, Weapon.GameObject.Transform.Position );

		Weapon.Tags.Add( "zooming" );

		if ( Weapon.ViewModel.IsValid() )
		{
			Weapon.ViewModel.GameObject.Enabled = false;
		}
	}

	protected void EndZoom()
	{
		if ( renderHook is not null )
			renderHook.Dispose();

		if ( UnzoomSound is not null && Weapon.IsValid() )
			Sound.Play( UnzoomSound, Weapon.GameObject.Transform.Position );

		ZoomLevel = 0;

		if ( Weapon.IsValid() )
		{
			Weapon.Tags.Remove( "zooming" );

			if ( Weapon.ViewModel.IsValid() )
			{
				Weapon.ViewModel.GameObject.Enabled = true;
			}
		}

		AnglesLerp = new Angles();
		BlurLerp = 1.0f;
	}

	private void OnZoomChanged( int oldValue, int newValue )
	{
		if ( oldValue == 0 ) StartZoom();
		else if ( newValue == 0) EndZoom();
	}

	public void RenderEffect( SceneCamera camera )
	{
		RenderAttributes attrs = new RenderAttributes();

		attrs.Set( "BlurAmount", BlurLerp );
		attrs.Set( "Offset", new Vector2( AnglesLerp.yaw, -AnglesLerp.pitch ) * AngleOffsetScale );

		Graphics.Blit( ScopeOverlay, attrs );
	}

	protected override void OnInput()
	{
		if ( ZoomLevel < ZoomLevels.Count )
		{
			ZoomLevel++;
		}
		else
		{
			ZoomLevel = 0;
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

	protected override void OnUpdate()
	{
		base.OnUpdate();
		var camera = Weapon.PlayerController.CameraController;

		if ( !IsZooming )
			return;

		if ( !CanAim() )
		{
			EndZoom();
		}

		if ( Weapon.PlayerController.CurrentWeapon != Weapon )
		{
			EndZoom();
		}

		camera.AddFieldOfViewOffset( ZoomLevels[ZoomLevel - 1] );
		Weapon.PlayerController.AimDampening /= (ZoomLevel * ZoomLevel) + 1;

		{
			var cc = Weapon.PlayerController.CharacterController;

			float velocity = Weapon.PlayerController.CharacterController.Velocity.Length / 25.0f;
			float blur = 1.0f / (velocity + 1.0f);
			blur = MathX.Clamp( blur, 0.1f, 1.0f );

			if ( !cc.IsOnGround )
				blur = 0.1f;

			if ( blur > BlurLerp )
				BlurLerp = BlurLerp.LerpTo( blur, Time.Delta * 1.0f );
			else
				BlurLerp = BlurLerp.LerpTo( blur, Time.Delta * 10.0f );

			var angles = Weapon.PlayerController.EyeAngles;
			var delta = angles - LastAngles;

			AnglesLerp = AnglesLerp.LerpTo( delta, Time.Delta * 10.0f );
			LastAngles= angles;
		}

	}
};
