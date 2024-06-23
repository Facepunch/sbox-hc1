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
	/// Is this player in spectate mode?
	/// </summary>
	[Sync] public bool IsSpectating { get; private set; } = false;
	
	/// <summary>
	/// How long since the player last respawned?
	/// </summary>
	public TimeSince TimeSinceLastRespawn { get; private set; }

	public override void Kill()
	{
		if ( Networking.IsHost )
		{
			HealthComponent.State = LifeState.Dead;
			ArmorComponent.HasHelmet = false;
			ArmorComponent.Armor = 0f;
		}

		EnableRagdoll();

		if ( IsProxy )
			return;

		Holster();

		_previousVelocity = Vector3.Zero;
		IsSpectating = true;
		CameraController.Mode = CameraMode.ThirdPerson;
	}

	public override void Respawn()
	{
		_previousVelocity = Vector3.Zero;

		Log.Info( $"Respawn( {GameObject.Name} ({DisplayName}, {Team}) )" );

		Body.DamageTakenForce = Vector3.Zero;

		ResetBody();

		if ( Networking.IsHost )
		{
			HealthComponent.Health = HealthComponent.MaxHealth;
			HealthComponent.State = LifeState.Alive;
			HealthComponent.RespawnState = RespawnState.None;

			ArmorComponent.HasHelmet = false;
			ArmorComponent.Armor = 0f;
		}
		
		TimeSinceLastRespawn = 0f;

		if ( !IsProxy )
		{
			GameMode.Instance?.SendSpawnConfirmation( Id );
		}

		if ( IsProxy )
			return;

		// Conna: we're not spectating if we just respawned.
		IsSpectating = false;

		// :S
		if ( GameMode.Instance.Get<ISpawnAssigner>() is { } spawnAssigner )
		{
			Teleport( spawnAssigner.GetSpawnPoint( this ) );
		}

		// Re-possess if we need to.
		if ( !PlayerState.IsBot )
		{
			PlayerState.Possess( this );
		}
	}

	public void Teleport( Transform transform )
	{
		Teleport( transform.Position, transform.Rotation );
	}

	public void Teleport( Vector3 position, Rotation rotation )
	{
		Transform.World = new( position, rotation );
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
