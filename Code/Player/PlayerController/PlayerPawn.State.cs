using Sandbox.Diagnostics;

namespace Facepunch;

public partial class PlayerPawn
{
	/// <summary>
	/// The player's health component
	/// </summary>
	[RequireComponent] public ArmorComponent ArmorComponent { get; private set; }

	/// <summary>
	/// The player's inventory, items, etc.
	/// </summary>
	[RequireComponent] public PlayerInventory Inventory { get; private set; }
	
	/// <summary>
	/// How long since the player last respawned?
	/// </summary>
	[HostSync] public TimeSince TimeSinceLastRespawn { get; private set; }

	public override void Kill()
	{
		// temp:
		if (Networking.IsHost)
		{
			PlayerState.RespawnState = RespawnState.Requested;
			GameObject.Destroy();
			return;
		}

		if ( Networking.IsHost )
		{
			HealthComponent.State = LifeState.Dead;
			ArmorComponent.HasHelmet = false;
			ArmorComponent.Armor = 0f;

			PlayerState.RespawnState = RespawnState.Requested;
		}

		EnableRagdoll();

		if ( IsProxy )
			return;

		Holster();

		_previousVelocity = Vector3.Zero;
		CameraController.Mode = CameraMode.ThirdPerson;
	}

	public override void Respawn()
	{
		_previousVelocity = Vector3.Zero;
		Body.DamageTakenForce = Vector3.Zero;

		ResetBody();

		if ( !Networking.IsHost )
			return;

		HealthComponent.Health = HealthComponent.MaxHealth;
		HealthComponent.State = LifeState.Alive;

		ArmorComponent.HasHelmet = false;
		ArmorComponent.Armor = 0f;

		// :S
		if ( GameMode.Instance.Get<ISpawnAssigner>() is { } spawnAssigner )
		{
			var s = spawnAssigner.GetSpawnPoint( this );
			Teleport( s );
		}

		using ( Rpc.FilterInclude( Network.OwnerConnection ) )
		{
			OnClientRespawn();
		}

		TimeSinceLastRespawn = 0f;
	}

	[Broadcast]
	public void OnClientRespawn()
	{
		if ( PlayerState.IsLocalPlayer )
		{ 
			Possess();
			OnPossess();
		}

		GameMode.Instance?.SendSpawnConfirmation( Id );
	}


	public void Teleport( Transform transform )
	{
		Teleport( transform.Position, transform.Rotation );
	}

	[Authority]
	public void Teleport( Vector3 position, Rotation rotation )
	{
		Transform.World = new( position, Rotation.Identity );
		Transform.ClearInterpolation();
		EyeAngles = rotation.Angles();

		if ( CharacterController.IsValid() )
			CharacterController.Velocity = Vector3.Zero;
	}
	
	private void EnableRagdoll()
	{
		Body.SetRagdoll( true );
		PlayerBoxCollider.Enabled = false;
	}

	private void ResetBody()
	{
		Body.SetRagdoll( false );
		Body.DamageTakenForce = Vector3.Zero;
		PlayerBoxCollider.Enabled = true;

		Outfitter.OnResetState( this );

		GameObject.Tags.Set( "invis", false );
	}
}
