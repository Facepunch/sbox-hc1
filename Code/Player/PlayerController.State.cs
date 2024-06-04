using Sandbox.Diagnostics;

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
	/// Is this player in spectate mode?
	/// </summary>
	[Sync] public bool IsSpectating { get; private set; }
	
	/// <summary>
	/// How long since the player last respawned?
	/// </summary>
	public TimeSince TimeSinceLastRespawn { get; private set; }

	void IRespawnable.Kill() => Kill();

	[Broadcast( NetPermission.HostOnly )]
	public void Kill( bool enableRagdoll = true )
	{
		if ( Networking.IsHost )
		{
			HealthComponent.State = LifeState.Dead;
			HealthComponent.HasHelmet = false;
			HealthComponent.Armor = 0f;
			Inventory.Clear();
		}

		if ( enableRagdoll )
			EnableRagdoll();
		else
			GameObject.Tags.Set( "invis", true );

		if ( IsProxy || IsBot )
			return;

		InBuyMenu = false;
		IsSpectating = true;
		CameraController.Mode = CameraMode.ThirdPerson;
	}

	void IRespawnable.Respawn()
	{
		if ( Networking.IsHost )
			Respawn();
	}

	/// <summary>
	/// Can be called on the host when creating the player before network spawning it. This
	/// seems like duplicating code but for now it's best practice to ensure state is up-to-date
	/// before clients receive it from the host instead of sending RPCs after spawning it in the same
	/// tick.
	/// </summary>
	public void Initialize()
	{
		Assert.True( Networking.IsHost );

		HealthComponent.State = LifeState.Dead;
		HealthComponent.HasHelmet = false;
		HealthComponent.Armor = 0f;
		HealthComponent.RespawnState = RespawnState.None;

		GameObject.Tags.Set( "invis", true );

		CameraController.Mode = CameraMode.ThirdPerson;
		IsSpectating = true;
		InBuyMenu = false;
	}

	[Broadcast( NetPermission.HostOnly )]
	public void Respawn()
	{
		Log.Info( $"Respawn( {GameObject.Name} ({GetPlayerName()}, {TeamComponent.Team}) )" );

		if ( TeamComponent.Team is Team.Terrorist or Team.CounterTerrorist )
			ResetBody();

		if ( Networking.IsHost )
		{
			HealthComponent.Health = 100f;
			HealthComponent.State = LifeState.Alive;
			HealthComponent.RespawnState = RespawnState.None;
		}
		
		TimeSinceLastRespawn = 0f;

		if ( IsProxy || IsBot )
			return;

		GameMode.Instance?.SendSpawnConfirmation();
		(this as IPawn).Possess();
		
		// Conna: we're not spectating if we just respawned.
		IsSpectating = false;
	}

	public void Teleport( Transform transform )
	{
		Assert.True( Networking.IsHost );
		Teleport( transform.Position, transform.Rotation );
	}

	[Authority( NetPermission.HostOnly )]
	public void Teleport( Vector3 position, Rotation rotation )
	{
		Transform.World = new( position, rotation );
		Transform.ClearInterpolation();
		EyeAngles = rotation.Angles();
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
