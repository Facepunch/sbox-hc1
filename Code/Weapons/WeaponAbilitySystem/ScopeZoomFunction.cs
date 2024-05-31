using Sandbox;

namespace Facepunch;

public class ScopeZoomFunction : InputActionWeaponFunction
{
	[Property] public Material ScopeOverlay { get; set; }
	[Property] public SoundEvent ZoomSound { get; set; }
	[Property] public SoundEvent UnzoomSound { get; set;}
	[Property] public int NumZoomLevels { get; set; } = 2;
	
	IDisposable renderHook;

	internal int ZoomLevel { get; private set; } = 0;

	protected void StartZoom( int level = 0 )
	{

		renderHook?.Dispose();
		renderHook = null;

		var camera = Weapon.PlayerController.CameraController;

		//renderHook = camera.AddHookAfterTransparent( "Scope", 100, RenderEffect );

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
	}

	public void RenderEffect( SceneCamera camera )
	{
		RenderAttributes attrs = new RenderAttributes();

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

		if( !CanAim() && ZoomLevel > 0 )
		{
			EndZoom();
		}

		camera.AddFieldOfViewOffset( ZoomLevel * 30 );

		Weapon.PlayerController.AimDampening /= ( ZoomLevel * ZoomLevel ) + 1;
	}
};
