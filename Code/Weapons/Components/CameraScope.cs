using System.Text.Json.Serialization;

namespace Facepunch;

[Title( "3D Scope" ), Group( "Weapon Components" )]
public class CameraScope : Component
{
	[Property] public Material ScopeMaterial { get; set; }
	[Property] public CameraComponent RenderCamera { get; set; }
	[RequireComponent] public ModelRenderer Renderer { get; set; }

	public Equipment Equipment => GetComponentInParent<Equipment>();

	[Property]
	public Texture ReticleTexture { get; set; }

	[Property, JsonIgnore]
	public Texture MyTexture { get; set; }

	protected override void OnEnabled()
	{
		Renderer.SetMaterial( ScopeMaterial );
		MyTexture ??= Texture.CreateRenderTarget( "3d_scope", ImageFormat.RGBA8888, 512 );
		RenderCamera.RenderTarget = MyTexture;
	}

	protected override void OnPreRender()
	{
		var isFarAway = Renderer.WorldPosition.Distance( Scene.Camera.WorldPosition ) > 5f;
		RenderCamera.Enabled = !isFarAway;
		Renderer.Tint = Renderer.Tint.WithAlpha( isFarAway ? 0.0f : 1.0f );

		if ( isFarAway )
			return;

		RenderCamera.RenderTarget.Clear( Color.Transparent );

		Renderer.SceneObject.Attributes.Set( "ReflectionTexture", MyTexture );

		float fov = RenderCamera.FieldOfView;
		Renderer.SceneObject.Attributes.Set( "ScopeFOV", fov );

		// Are we aiming?
		if ( Equipment.HasFlag( EquipmentFlags.Aiming ) )
		{
			if ( Input.Keyboard.Down( "uparrow" ) )
			{
				RenderCamera.FieldOfView += Time.Delta * 15f;
			}

			if ( Input.Keyboard.Down( "downarrow" ) )
			{
				RenderCamera.FieldOfView -= Time.Delta * 15f;
			}

			RenderCamera.FieldOfView = Math.Clamp( RenderCamera.FieldOfView, 10f, 30f );

			// Dampen aiming
			Equipment.Owner.AimDampening /= 1 - RenderCamera.FieldOfView.Remap( 10, 30, 0, 1 ) + 1;
		}

		// Get world-space scope data
		var scopePos = RenderCamera.WorldPosition;
		var scopeForward = RenderCamera.WorldRotation.Forward;
		var scopeRight = RenderCamera.WorldRotation.Right;
		var scopeUp = RenderCamera.WorldRotation.Up;

		// Get player eye position
		var eyePos = Scene.Camera.WorldPosition;

		// Vector from scope to eye
		var toEye = eyePos - scopePos;

		// Project onto local axes
		float localX = toEye.Dot( scopeRight );
		float localY = toEye.Dot( scopeUp );
		float depthZ = toEye.Dot( scopeForward );

		float apertureRadius = 0.5f;
		float localOffsetX = Math.Clamp( localX / apertureRadius, -5.0f, 5.0f );
		float localOffsetY = Math.Clamp( localY / apertureRadius, -5.0f, 5.0f );

		// Send to shader
		Renderer.SceneObject.Attributes.Set( "ScopeEyeOffset", new Vector2( localOffsetX, localOffsetY ) );

		// DebugOverlay.Texture( MyTexture, new Rect( 20, Screen.Height * 0.5f ), Color.White );
	}
};
