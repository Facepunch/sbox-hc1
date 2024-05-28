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

	[Broadcast]
	public void SetBodyVisible( bool visible )
	{
		Body.SetRagdoll( !visible );
		PlayerBoxCollider.Enabled = visible;
	}

	void IRespawnable.Respawn()
	{
		HealthComponent.Health = 100;
		Respawn();
	}

	[Authority( NetPermission.HostOnly )]
	public void Respawn()
	{
		Log.Info( $"Respawn( {GameObject.Name} ({Network.OwnerConnection?.DisplayName}, {TeamComponent.Team}) )" );

		SetBodyVisible( true );

		Inventory.Clear();

		HealthComponent.Health = 100;
		HealthComponent.State = LifeState.Alive;

		var spawn = GameMode.Instance.GetSpawnTransform( TeamComponent.Team );

		Transform.World = new Transform( spawn.Position, spawn.Rotation );

		GameMode.Instance?.HandlePlayerSpawn();
	}
}
