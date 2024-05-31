using Sandbox;

namespace Facepunch;

public class ScopeZoomFunction : InputActionWeaponFunction
{
	[Property] public Material ScopeOverlay { get; set; }
	[Property] public SoundEvent ZoomSound { get; set; }
	[Property] public SoundEvent UnzoomSound { get; set;}
	[Property] public int NumZoomLevels { get; set; } = 2;
	
	IDisposable renderHook;

	private int ZoomLevel { get; set; } = 0;
	public bool IsZooming => ZoomLevel > 0;
	private float BlurLerp { get; set; } = 1.0f;

	private Angles LastAngles;
	private Angles AnglesLerp;
	[Property] private float AngleOffsetScale { get; set; } = 0.01f;

	protected void StartZoom( int level = 0 )
	{

		renderHook?.Dispose();
		renderHook = null;

		var camera = Weapon.PlayerController.CameraController;

		if ( ScopeOverlay is not null )
			renderHook = camera.Camera.AddHookAfterTransparent( "Scope", 100, RenderEffect );

		if( ZoomSound is not null )
			Sound.Play( ZoomSound, Weapon.GameObject.Transform.Position );

		ZoomLevel = level;
		Weapon.Tags.Add( "zooming" );
		Weapon.ViewModel.GameObject.Enabled = false;
	}

	protected void EndZoom()
	{
		if( renderHook is not null)
			renderHook.Dispose();

		if( UnzoomSound is not null )
			Sound.Play( UnzoomSound, Weapon.GameObject.Transform.Position );

		ZoomLevel = 0;
		Weapon.Tags.Remove( "zooming" );
		Weapon.ViewModel.GameObject.Enabled = true;

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

	protected override void OnFunctionExecute()
	{
		if( ZoomLevel < NumZoomLevels )
		{
			StartZoom( ZoomLevel+1 );
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

		camera.AddFieldOfViewOffset( ZoomLevel * 30 );
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
