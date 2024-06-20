namespace Facepunch;

public class FlashbangEffect : Component
{
	[Property] public float LifeTime { get; set; }
	
	private TimeUntil TimeUntilEnd { get; set; }
	private MotionBlur MotionBlur { get; set; }
	
	protected override void OnEnabled()
	{
		MotionBlur = Components.Create<MotionBlur>();
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		MotionBlur?.Destroy();
		base.OnDisabled();
	}

	protected override void OnStart()
	{
		TimeUntilEnd = LifeTime;
		MotionBlur.Samples = 2;
		MotionBlur.Scale = 0.8f;
		
		base.OnStart();
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
