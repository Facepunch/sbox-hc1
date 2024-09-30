using Facepunch.UI;
using Sandbox.Audio;
using Sandbox.Utility;

namespace Facepunch;

public class FlashbangEffect : Component
{
	[Property] public SoundEvent SoundEffect { get; set; }
	[Property] public float LifeTime { get; set; }
	
	private TimeUntil TimeUntilEnd { get; set; }
	private FlashbangOverlay Overlay { get; set; }
	private ChromaticAberration Aberration { get; set; }
	private CameraComponent Camera { get; set; }
	private Bloom Bloom { get; set; }
	
	private DspProcessor DspProcessor { get; set; }
	private SoundHandle Sound { get; set; }
	private IDisposable RenderHook { get; set; }
	private Texture FreezeFrame { get; set; }
	private float RenderAlpha { get; set; }
	
	protected override void OnEnabled()
	{
		Bloom ??= AddComponent<Bloom>();
		Overlay ??= AddComponent<FlashbangOverlay>();
		Aberration ??= AddComponent<ChromaticAberration>();
		Camera ??= AddComponent<CameraComponent>();
		DspProcessor ??= new( "weird.4" );
		
		Mixer.Master.AddProcessor( DspProcessor );
		
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		if ( Sound.IsValid() )
			Sound.Volume = 0f;
		
		Mixer.Master.RemoveProcessor( DspProcessor );
		
		base.OnDisabled();
	}

	protected override void OnDestroy()
	{
		if ( Bloom.IsValid() ) Bloom.Destroy();
		if ( Overlay.IsValid() ) Overlay?.Destroy();
		if ( Aberration.IsValid() ) Aberration?.Destroy();
		
		RenderHook?.Dispose();
		Sound?.Stop();
		FreezeFrame?.Dispose();
	}

	protected override void OnStart()
	{
		TimeUntilEnd = LifeTime;
		
		Bloom.Mode = SceneCamera.BloomAccessor.BloomMode.Screen;
		Bloom.Threshold = 0f;
		Bloom.ThresholdWidth = 0f;
		Bloom.Strength = 10f;

		Aberration.Scale = 1f;
		Aberration.Offset = new( 6f, 10f, 3f );

		Overlay.EndTime = LifeTime * 0.6f;

		if ( SoundEffect is not null )
		{
			Sound = Sandbox.Sound.Play( SoundEffect );
			Sound.Volume = 1f;
		}
		
		RenderHook = Camera.AddHookAfterTransparent( "Flashbang", 101, RenderEffect );
		RenderAlpha = 1f;
		
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		var f = TimeUntilEnd.Relative / LifeTime;
		Aberration.Scale = f;
		Bloom.Strength = 10f * f;
		DspProcessor.Mix = f;
		
		if ( Sound.IsValid() )
			Sound.Volume = f;

		RenderAlpha = Easing.EaseOut( f );
		
		base.OnUpdate();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		
		if ( TimeUntilEnd )
		{
			Destroy();
		}
	}
	
	private void RenderEffect( SceneCamera camera )
	{
		if ( FreezeFrame is null )
		{
			Graphics.GrabFrameTexture( "Flashbang" );
			var texture = Graphics.Attributes.GetTexture( "Flashbang" );
			
			FreezeFrame = Texture.Create( texture.Width, texture.Height, texture.ImageFormat )
				.WithMips( texture.Mips )
				.Finish();
			
			Graphics.CopyTexture( texture, FreezeFrame );
		}
		
		var rect = new Rect( 0f, 0f, FreezeFrame.Width, FreezeFrame.Height );
		Graphics.Attributes.Set( "LayerMat", Matrix.Identity );
		Graphics.Attributes.Set( "Texture", FreezeFrame );
		Graphics.Attributes.SetComboEnum( "D_BLENDMODE", BlendMode.Normal );
		Graphics.DrawQuad( rect, Material.UI.Basic, new( 1f, 1f, 1f, RenderAlpha ) );
	}
}
