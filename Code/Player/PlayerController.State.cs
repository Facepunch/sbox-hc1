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
		if ( Networking.IsHost )
		{
			HealthComponent.State = CanRespawn ? LifeState.Respawning : LifeState.Dead;
			Inventory.Clear();
		}
		
		SetBodyVisible( false );
		InBuyMenu = false;
	}

	[Broadcast]
	public void SetBodyVisible( bool visible )
	{
		Body.SetRagdoll( !visible );
		PlayerBoxCollider.Enabled = visible;
	}

	void IRespawnable.Respawn()
	{
		if ( Networking.IsHost )
		{
			HealthComponent.Health = 100f;
			Respawn();
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	public void Respawn()
	{
		Log.Info( $"Respawn( {GameObject.Name} ({Network.OwnerConnection?.DisplayName}, {TeamComponent.Team}) )" );

		SetBodyVisible( true );

		if ( Networking.IsHost )
		{
			if ( HealthComponent.State != LifeState.Alive )
			{
				Inventory.Clear();
				HealthComponent.Armor = 0f;
			}

			HealthComponent.Health = 100f;
			HealthComponent.State = LifeState.Alive;
		}

		if ( !IsProxy )
		{
			GameMode.Instance?.HandlePlayerSpawn();
		}
	}

	public void Teleport( Transform transform )
	{
		Teleport( transform.Position, transform.Rotation );
	}

	[Broadcast( NetPermission.HostOnly )]
	public void Teleport( Vector3 position, Rotation rotation )
	{
		Transform.World = new( position, rotation );
	}
}
