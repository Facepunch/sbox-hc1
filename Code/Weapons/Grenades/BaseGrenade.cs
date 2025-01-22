namespace Facepunch;

public abstract class BaseGrenade : Component
{
	[Property] public float Lifetime { get; set; }
	[Property] public GameObject PrefabOnExplode { get; set; }
	[Property] public bool CanDealDamage { get; set; }
	[Sync] public PlayerPawn Player { get; set; }

	/// <summary>
	/// Is this player an enemy of the viewer?
	/// </summary>
	public bool IsEnemy => Client.Viewer.Team != Player.Team;

	private TimeSince TimeSinceCreated { get; set; }

	protected override void OnStart()
	{
		TimeSinceCreated = 0f;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy || !CanExplode() ) return;
		
		if ( TimeSinceCreated > Lifetime )
		{
			Explode();
		}
	}

	protected virtual bool CanExplode()
	{
		return true;
	}

	[Rpc.Broadcast]
	protected virtual void Explode()
	{
		if ( PrefabOnExplode.IsValid() )
			PrefabOnExplode.Clone( WorldPosition );

		if ( IsProxy ) return;
		GameObject.Destroy();
	}
}
