namespace Facepunch;

[Title( "Molotov Fire Node" )]
public class MolotovFireNode : Component
{
	public static List<MolotovFireNode> All { get; set; } = new();

	[Property] public float LifeTime { get; set; } = 10f;
	
	
	private TimeUntil TimeUntilExtinguish { get; set; }
	private bool IsExtinguishing { get; set; }

	public async void Extinguish()
	{
		if ( IsExtinguishing )
			return;

		IsExtinguishing = true;

		var emitters = Components.GetAll<ParticleEmitter>();
		foreach ( var emitter in emitters )
		{
			emitter.Enabled = false;
		}

		await Task.DelaySeconds( 1f );
		GameObject.Destroy();
	}

	protected override void OnStart()
	{
		TimeUntilExtinguish = LifeTime;
		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		if ( TimeUntilExtinguish && !IsExtinguishing )
		{
			Extinguish();
		}
		
		base.OnFixedUpdate();
	}

	protected override void OnEnabled()
	{
		All.Add( this );
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		All.Remove( this );
		base.OnDisabled();
	}
}
