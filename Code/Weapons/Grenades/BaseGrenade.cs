namespace Facepunch;

public abstract class BaseGrenade : Component
{
	[Sync] public Guid ThrowerId { get; set; }
	[Property] public float Lifetime { get; set; }
	[Property] public GameObject PrefabOnExplode { get; set; }

	public PlayerController Player => (PlayerController)Scene.Directory.FindComponentByGuid( ThrowerId );

	/// <summary>
	/// Is this player an enemy of the viewer?
	/// </summary>
	public bool IsEnemy => GameUtils.Viewer.IsValid() && GameUtils.Viewer.Team != Player.TeamComponent.Team;

	private TimeSince TimeSinceCreated { get; set; }

	protected override void OnStart()
	{
		TimeSinceCreated = 0f;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		
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

		if ( IsProxy ) return;
		GameObject.Destroy();
	}
}
