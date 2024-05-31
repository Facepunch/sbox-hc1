namespace Facepunch;

public abstract class BaseGrenade : Component
{
	// Conna: store the thrower as their id because they might have disconnected by the time it explodes.
	public Guid ThrowerId { get; set; }
	
	[Property] public float Lifetime { get; set; }
	[Property] public GameObject PrefabOnExplode { get; set; }
	
	private TimeSince TimeSinceCreated { get; set; }

	protected override void OnStart()
	{
		TimeSinceCreated = 0f;
	}

	protected override void OnUpdate()
	{
		// Conna: only the host should ever handle explosions.
		if ( !Networking.IsHost ) return;
		
		if ( TimeSinceCreated > Lifetime )
		{
			Explode();
		}
	}

	[Broadcast]
	protected virtual void Explode()
	{
		if ( PrefabOnExplode.IsValid() )
		{
			PrefabOnExplode.Clone( Transform.Position );
		}

		if ( Networking.IsHost )
		{
			GameObject.Destroy();
		}
	}
}
