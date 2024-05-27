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
		NetDePossess();

		// TODO: Turn off the body (or a death anim)
		// Kill player inventory

		SetBodyVisible( false );

		HealthComponent.State = CanRespawn ? LifeState.Respawning : LifeState.Dead;
	}

	public void SetBodyVisible( bool visible )
	{
		Body.GameObject.Enabled = visible;
	}

	void IRespawnable.Respawn()
	{
		HealthComponent.Health = 100;

		Inventory.Clear();
		Inventory.Setup();

		SetBodyVisible( true );
	}

	[Authority( NetPermission.HostOnly )]
	private void MoveToSpawnPoint( Vector3 position, Rotation rotation )
	{
		Transform.World = new Transform( position, rotation );
	}

	public void Respawn()
	{
		HealthComponent.State = LifeState.Alive;

		var spawn = GameMode.Instance.GetSpawnTransform( TeamComponent.Team );

		MoveToSpawnPoint( spawn.Position, spawn.Rotation );
		NetPossess();
	}
}
