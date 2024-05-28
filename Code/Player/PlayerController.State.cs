namespace Facepunch;

public partial class PlayerController
{
	/// <summary>
	/// The player's health component
	/// </summary>
	[RequireComponent] public HealthComponent HealthComponent { get; private set; }

	/// <summary>
	/// The player's inventory, items, etc.
	/// </summary>
	[RequireComponent] public PlayerInventory Inventory { get; private set; }

    /// <summary>
    /// Component describing which team the player is on.
    /// </summary>
    [RequireComponent] public TeamComponent TeamComponent { get; private set; }

    public void Kill()
	{
		Inventory.Clear();
		SetBodyVisible( false );

		InBuyMenu = false;

		HealthComponent.State = CanRespawn ? LifeState.Respawning : LifeState.Dead;
	}

	public void SetBodyVisible( bool visible )
	{
		Body.GameObject.Enabled = visible;
	}

	void IRespawnable.Respawn()
	{
		HealthComponent.Health = 100;
		Respawn();
	}

	[Authority( NetPermission.HostOnly )]
	private void MoveToSpawnPoint( Vector3 position, Rotation rotation )
	{
		Transform.World = new Transform( position, rotation );
	}

	public void Respawn()
	{
		Inventory.Clear();

		HealthComponent.State = LifeState.Alive;

		var spawn = GameMode.Instance.GetSpawnTransform( TeamComponent.Team );

		SetBodyVisible( true );

		MoveToSpawnPoint( spawn.Position, spawn.Rotation );
		NetPossess();

		_ = GameMode.Instance?.HandlePlayerSpawn( this );
	}
}
