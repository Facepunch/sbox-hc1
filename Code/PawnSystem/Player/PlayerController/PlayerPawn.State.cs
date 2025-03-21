using Sandbox.Diagnostics;
using Sandbox.Events;

namespace Facepunch;

public record OnPlayerRagdolledEvent : IGameEvent
{
	public float DestroyTime { get; set; } = 0f;
}

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
	[Sync( SyncFlags.FromHost )] public TimeSince TimeSinceLastRespawn { get; private set; }

	public override void OnKill( DamageInfo damageInfo )
	{
		if ( Networking.IsHost )
		{
			ArmorComponent.HasHelmet = false;
			ArmorComponent.Armor = 0f;

			var defaultRespawnState = Scene.GetAllComponents<DefaultRespawnState>().FirstOrDefault();
			if ( defaultRespawnState.IsValid() )
			{
				Client.RespawnState = defaultRespawnState.RespawnState;
			}
			else
			{
				// on join, spawn right now if we can
				Client.RespawnState = RespawnState.Requested;
			}

			Inventory.Clear();
			CreateRagdoll();
		}

		PlayerBoxCollider.Enabled = false;

		if ( IsProxy )
			return;

		Client.OnKill( damageInfo );

		Holster();

		_previousVelocity = Vector3.Zero;
		CameraController.Mode = CameraMode.ThirdPerson;
	}

	public void SetSpawnPoint( SpawnPointInfo spawnPoint )
	{
		SpawnPosition = spawnPoint.Position;
		SpawnRotation = spawnPoint.Rotation;

		SpawnPointTags.Clear();

		foreach ( var tag in spawnPoint.Tags )
		{
			SpawnPointTags.Add( tag );
		}
	}

	public override void OnRespawn()
	{
		Assert.True( Networking.IsHost );

		OnHostRespawn();
		OnClientRespawn();
	}

	private void OnHostRespawn()
	{
		Assert.True( Networking.IsHost );

		_previousVelocity = Vector3.Zero;

		Teleport( SpawnPosition, SpawnRotation );

		if ( Body is not null )
		{
			Body.DamageTakenForce = Vector3.Zero;
		}

		if ( HealthComponent.State != LifeState.Alive )
		{
			ArmorComponent.HasHelmet = false;
			ArmorComponent.Armor = 0f;
		}

		HealthComponent.Health = HealthComponent.MaxHealth;

		TimeSinceLastRespawn = 0f;

		ResetBody();
		Scene.Dispatch( new PlayerSpawnedEvent( this ) );
	}

	[Rpc.Owner]
	private void OnClientRespawn()
	{
		if ( !Client.IsValid() )
			return;

		if ( Client.IsBot )
			return;

		Possess();
	}

	public void Teleport( Transform transform )
	{
		Teleport( WorldPosition, transform.Rotation );
	}

	[Rpc.Owner]
	public void Teleport( Vector3 position, Rotation rotation )
	{
		Transform.World = new( position, rotation );
		Transform.ClearInterpolation();
		EyeAngles = rotation.Angles();

		if ( CharacterController.IsValid() )
		{
			CharacterController.Velocity = Vector3.Zero;
			CharacterController.IsOnGround = true;
		}
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void CreateRagdoll()
	{
		if ( !Body.IsValid() )
			return;

		Body.SetRagdoll( true );
		Body.GameObject.SetParent( null, true );
		Body.GameObject.Name = $"Ragdoll ({DisplayName})";

		var ev = new OnPlayerRagdolledEvent();
		Scene.Dispatch( ev );

		if ( ev.DestroyTime > 0f )
		{
			var comp = Body.Components.Create<TimedDestroyComponent>();
			comp.Time = ev.DestroyTime;
		}
		else
		{
			Body.Components.Create<DestroyBetweenRounds>();
		}

		Body = null;
	}

	private void ResetBody()
	{
		if ( Body is not null )
		{
			Body.DamageTakenForce = Vector3.Zero;
		}

		PlayerBoxCollider.Enabled = true;

		var outfitter = GetComponentInChildren<HumanOutfitter>();

		if ( outfitter.IsValid() )
			outfitter.UpdateFromTeam( Team );
	}
}
