using Facepunch.UI;

namespace Facepunch;

public class FlashbangEffect : Component
{
	[Property] public float LifeTime { get; set; }
	
	private TimeUntil TimeUntilEnd { get; set; }
	private FlashbangOverlay Overlay { get; set; }
	private Bloom Bloom { get; set; }
	
	protected override void OnEnabled()
	{
		Bloom = Components.Create<Bloom>();
		Overlay = Components.Create<FlashbangOverlay>();
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		Bloom?.Destroy();
		Overlay?.Destroy();
		
		base.OnDisabled();
	}

	protected override void OnStart()
	{
		TimeUntilEnd = LifeTime;
		
		Bloom.Mode = SceneCamera.BloomAccessor.BloomMode.Screen;
		Bloom.Threshold = 0f;
		Bloom.ThresholdWidth = 0f;
		Bloom.Strength = 10f;

		Overlay.EndTime = LifeTime;
		
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		var f = TimeUntilEnd.Relative / LifeTime;
		Bloom.Strength = 10f * f;
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
}
