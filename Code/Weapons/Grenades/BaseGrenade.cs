namespace Facepunch;

public abstract class BaseGrenade : Component
{
	public PlayerController Owner { get; set; }

	public TimeSince TimeSinceCreated { get; set; }
	[Property] public float Lifetime { get; set; }

	[Property] public GameObject PrefabOnExplode { get; set; }

	protected override void OnStart()
	{
		TimeSinceCreated = 0;
	}

	protected override void OnUpdate()
	{
		if ( TimeSinceCreated > Lifetime )
		{
			Explode();
		}
	}

	protected virtual void Explode()
	{
		if ( PrefabOnExplode.IsValid() )
		{
			PrefabOnExplode.Clone( Transform.Position, Transform.Rotation );
		}

		GameObject.Destroy();
	}
}
