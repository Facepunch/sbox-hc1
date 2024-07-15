namespace Facepunch;

public enum IdleType
{
	Default,
	WithRifle,
	WithKnife,
	WithPistol
}

public sealed class IdleAnimator : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public IdleType IdleType { get; set; }

	protected override void OnFixedUpdate()
	{
		if ( Renderer.IsValid() )
		{
			Renderer.Set( "idle_type", (int)IdleType );
		}
	}
}
