using Sandbox.Diagnostics;
using Sandbox.Events;
using System.Text.Json.Serialization;

namespace Facepunch;

public partial class PlayerState : Component
{
	/// <summary>
	/// The player we're currently in the view of (clientside).
	/// Usually the local player, apart from when spectating etc.
	/// </summary>
	public static PlayerState Viewer { get; private set; }

	/// <summary>
	/// Our local player on this client.
	/// </summary>
	public static PlayerState Local { get; private set; }

	// --

	/// <summary>
	/// The prefab to spawn when we want to make a player pawn for the player.
	/// </summary>
	[Property] public GameObject PlayerPawnPrefab { get; set; }

	/// <summary>
	/// Who owns this player state?
	/// </summary>
	[Sync, Property] public ulong SteamId { get; set; }

	/// <summary>
	/// We always want a reference to this player state's current player pawn. 
	/// They could have this AND be controlling a drone for example.
	/// </summary>
	public PlayerPawn PlayerPawn
	{
		get => Scene.Directory.FindComponentByGuid( playerPawnGuid ) as PlayerPawn;
		set => playerPawnGuid = value.Id;
	}
	[Sync] private Guid playerPawnGuid { get; set; } // Sync this so other people know what player belongs to who

	[Broadcast]
	public void RequestCreatePlayer()
	{
		if ( PlayerPawn.IsValid() )
			return;

		CreatePlayer();
	}

	public void CreatePlayer()
	{
		var prefab = PlayerPawnPrefab.Clone();
		var pawn = prefab.Components.Get<PlayerPawn>();
		pawn.PlayerState = this;
		prefab.NetworkSpawn( Network.OwnerConnection );

		PlayerPawn = pawn;
		if ( IsBot )
			Pawn = pawn;

		using ( Rpc.FilterInclude(Network.OwnerConnection) )
		{
			OnClientRespawn();
		}

		pawn.Respawn();
	}

	[Broadcast]
	public void OnClientRespawn()
	{
		if ( !IsBot )
		{
			Possess();
		}

		GameMode.Instance?.SendSpawnConfirmation( Pawn.Id );
	}


	public void DestroyPlayer()
	{
		if ( Pawn.IsValid() )
		{
			Pawn.GameObject.Destroy();
		}
	}

	/// <summary>
	/// Unique Ids of this player
	/// </summary>
	[RequireComponent] public PlayerId PlayerId { get; private set; }

	/// <summary>
	/// The team this player is on.
	/// </summary>
	[Property, Group( "Setup" ), HostSync, Change( nameof( OnTeamPropertyChanged ) )]
	public Team Team { get; set; }

	/// <summary>
	/// Called when <see cref="Team"/> changes across the network.
	/// </summary>
	/// <param name="before"></param>
	/// <param name="after"></param>
	private void OnTeamPropertyChanged( Team before, Team after )
	{
		GameObject.Root.Dispatch( new TeamChangedEvent( before, after ) );

		// Send this to the pawn too if we haveo ne
		if ( PlayerPawn.IsValid() )
		{
			PlayerPawn.GameObject.Root.Dispatch( new TeamChangedEvent( before, after ) );
		}
	}

	/// <summary>
	/// Name of this player
	/// </summary>
	public string DisplayName => IsBot ? $"BOT {BotManager.Instance.GetName( BotId )}" : Network.OwnerConnection?.DisplayName ?? "";

	/// <summary>
	/// The pawn this player is currently in possession of (networked if it's networked)
	/// </summary>
	[Property] public Pawn Pawn { get; private set; }
	[HostSync, Property, JsonIgnore] private Guid pawnGuid { get; set; } = Guid.Empty;
	// todo: this should be an engine feature?

	/// <summary>
	/// Are we in the view of this player (clientside)
	/// </summary>
	public bool IsViewer => Viewer == this;

	/// <summary>
	/// Is this the local player for this client
	/// </summary>
	public bool IsLocalPlayer => !IsProxy && !IsBot;

	protected override void OnAwake()
	{
		Pawn = Scene.Directory.FindComponentByGuid( pawnGuid ) as Pawn;
	}

	protected override void OnStart()
	{
		// No player state?
		if ( !IsProxy && !IsBot && !Local.IsValid() )
		{
			Local = this;
			Viewer = this;
		}

		Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
	}

	/// <summary>
	/// Called from client when we've taken possession of a pawn.
	/// </summary>
	public void NotifyNetPossessed( Pawn pawn )
	{
		Assert.True( !IsProxy );
		NotifyNetPossessed( pawn.Id );
	}

	[Broadcast]
	private void NotifyNetPossessed( Guid guid )
	{
		if ( Networking.IsHost )
			pawnGuid = guid;

		// todo: this should be an engine feature?
		Pawn = Scene.Directory.FindComponentByGuid( guid ) as Pawn;

		if ( IsViewer && IsProxy )
		{
			Possess();
		}
	}

	public void Possess()
	{
		Viewer = this;

		if ( Pawn is null || IsLocalPlayer )
		{
			// Local player - always assume the controller
			Possess ( PlayerPawn );
		}
		else
		{
			// A remote player is possessing this player (spectating)
			// So enter the latest known pawn this player has possessed
			Possess( Pawn );
		}
	}

	public void Possess( Pawn pawn )
	{
		Assert.True( pawn.IsValid(), "PlayerState attempted Possess but has no Controller!" );

		DePossess();

		Pawn = pawn;
		Pawn.OnPossess();

		if ( IsLocalPlayer )
		{
			Pawn.PlayerState = this;
		}

		if ( !IsProxy )
		{
			// tell the host we've possessed this
			NotifyNetPossessed( Pawn.Id );
		}
	}

	public void DePossess()
	{
		if ( !Pawn.IsValid() )
			return;

		Pawn.OnDePossess();
		Pawn = null;
	}

	public void AssignTeam( Team team )
	{
		if ( !Networking.IsHost )
			return;

		Team = team;

		Scene.Dispatch( new TeamAssignedEvent( this, team ) );
	}
}
