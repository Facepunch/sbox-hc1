using Sandbox;

namespace Facepunch;

public class ScopeZoomFunction : AimWeaponFunction
{
	[Property] public Material ScopeOverlay { get; set; }

	IDisposable renderHook;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		renderHook?.Dispose();
		renderHook = null;

		var camera = Weapon.PlayerController.CameraController.Camera;

		renderHook = camera.AddHookAfterTransparent( "Scope", 100, RenderEffect );
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		renderHook.Dispose();
	}

	public void RenderEffect( SceneCamera camera )
	{
		RenderAttributes attrs = new RenderAttributes();

		Graphics.Blit( ScopeOverlay, attrs );
	}
};
