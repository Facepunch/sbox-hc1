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

	/// <summary>
	/// Is this player in spectate mode
	/// </summary>
	public bool IsSpectating => HealthComponent.State == LifeState.Dead;

	public void Kill()
	{
		if ( Networking.IsHost )
		{
			HealthComponent.State = CanRespawn ? LifeState.Respawning : LifeState.Dead;
			Inventory.Clear();
		}

		if ( TeamComponent.Team == Team.Terrorist || TeamComponent.Team == Team.CounterTerrorist )
		{
			SetRagdoll();
		}
		else
		{
			Body.ShowBodyParts( BodyRenderMode.Hide );
		}

		InBuyMenu = false;
	}

	[Broadcast]
	public void SetRagdoll()
	{
		Body.SetRagdoll( true );
		PlayerBoxCollider.Enabled = false;
	}

	[Broadcast]
	public void ResetBody()
	{
		Body.SetRagdoll( false );
		PlayerBoxCollider.Enabled = true;
		Body.DamageTakenForce = Vector3.Zero;

		Outfitter.OnResetState( this );
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
		Log.Info( $"Respawn( {GameObject.Name} ({GetPlayerName()}, {TeamComponent.Team}) )" );

		if ( !IsSpectating )
			ResetBody();

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

		if ( !IsProxy && !IsBot )
		{
			GameMode.Instance?.HandlePlayerSpawn();
			(this as IPawn).Possess();
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
