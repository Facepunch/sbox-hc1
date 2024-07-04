namespace Facepunch;

[Title( "Smoke Grenade" )]
public partial class SmokeGrenade : BaseGrenade
{
	[Property] public GameObject FuseEffect { get; set; }
	[Property] public float FuseEffectDelay { get; set; } = 1.5f;
	[Property] public float ExtinguishRadius { get; set; } = 128f;
	
	private TimeUntil FuseEffectStartTime { get; set; }

	protected override void OnStart()
	{
		FuseEffectStartTime = FuseEffectDelay;
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		if ( FuseEffectStartTime && FuseEffect.IsValid() && !FuseEffect.Enabled )
		{
			FuseEffect.Enabled = true;
		}
		
		base.OnUpdate();
	}

	protected override void Explode()
	{
		base.Explode();

		var radius = ExtinguishRadius * ExtinguishRadius;

		foreach ( var node in MolotovFireNode.All )
		{
			if ( node.Transform.Position.DistanceSquared( Transform.Position ) <= radius )
			{
				node.Extinguish();
			}
		}
	}
}
