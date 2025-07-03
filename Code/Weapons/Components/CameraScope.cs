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

	[Property]
	public float Forward { get; set; } = 20;

	protected override void OnEnabled()
	{
		Renderer.SetMaterial( ScopeMaterial );
		MyTexture ??= Texture.CreateRenderTarget( "3d_scope", ImageFormat.RGBA8888, 512 );
		RenderCamera.RenderTarget = MyTexture;
		RenderCamera.GameObject.SetParent( GameObject );
	}

	protected override void OnPreRender()
	{
		var isFarAway = Renderer.WorldPosition.Distance( Scene.Camera.WorldPosition ) > 5f;
		RenderCamera.Enabled = !isFarAway;
		Renderer.Tint = Renderer.Tint.WithAlpha( isFarAway ? 0.0f : 1.0f );

		if ( isFarAway )
			return;

		var origin = GameObject.WorldPosition;
		var forward = RenderCamera.WorldRotation.Forward;
		var back = RenderCamera.WorldRotation.Backward;

		var tr = Scene.Trace.Ray( origin, origin + forward * Forward )
			.IgnoreGameObjectHierarchy( Equipment?.Owner.GameObject )
			.Radius( 1 )
			.Run();

		RenderCamera.WorldPosition = tr.EndPosition;

		//DebugOverlay.Line( new Line( tr.StartPosition, tr.EndPosition ), Color.Red, 0f );
		//DebugOverlay.Sphere( new Sphere( tr.EndPosition, 1.5f ), Color.Red, 0f );

		RenderCamera.RenderTarget.Clear( Color.Transparent );
		Renderer.SceneObject.Attributes.Set( "ReflectionTexture", MyTexture );

		// Are we aiming?
		if ( Equipment.IsValid() && Equipment.HasFlag( EquipmentFlags.Aiming ) )
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

		float fov = RenderCamera.FieldOfView;
		Renderer.SceneObject.Attributes.Set( "ScopeFOV", fov );

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
