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
	/// Who owns this player state?
	/// </summary>
	[HostSync, Property] public ulong SteamId { get; set; }

	/// <summary>
	/// The connection of this player
	/// </summary>
	public Connection Connection => Network.OwnerConnection;

	/// <summary>
	/// Name of this player
	/// </summary>
	public string DisplayName => IsBot ? $"BOT {BotManager.Instance.GetName( BotId )}" : Network.OwnerConnection?.DisplayName ?? "";

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
	/// Are we in the view of this player (clientside)
	/// </summary>
	public bool IsViewer => Viewer == this;

	/// <summary>
	/// Is this the local player for this client
	/// </summary>
	public bool IsLocalPlayer => !IsProxy && !IsBot;

	/// <summary>
	/// The main PlayerPawn of this player if one exists, will not change when the player possesses gadgets etc. (synced)
	/// </summary>
	public PlayerPawn PlayerPawn
	{
		get => Scene.Directory.FindComponentByGuid( playerPawnGuid ) as PlayerPawn;
		set => playerPawnGuid = value.Id;
	}
	[HostSync, JsonIgnore] private Guid playerPawnGuid { get; set; }

	/// <summary>
	/// The pawn this player is currently in possession of (synced - unless the pawn is not networked)
	/// </summary>
	public Pawn Pawn
	{
		get => Scene.Directory.FindComponentByGuid( pawnGuid ) as PlayerPawn;
		set => pawnGuid = value.Id;
	}
	[HostSync, JsonIgnore] private Guid pawnGuid { get; set; } = Guid.Empty;

	public void HostInit()
	{
		// on join, spawn right now if we can
		RespawnState = RespawnState.Immediate;
		SteamId = Connection.SteamId;
	}

	[Authority]
	public void ClientInit()
	{
		if ( IsBot )
			return;

		Local = this;
	}

	public void Kick()
	{
		if ( PlayerPawn.IsValid() )
		{
			PlayerPawn.GameObject.Destroy();
		}

		GameObject.Destroy();
		// todo: actually kick em
	}

	public static void OnPossess( Pawn pawn )
	{
		// called from Pawn when one is newly possessed, update Local and Viewer, invoke RPCs for observers

		if ( !pawn.IsProxy )
		{
			Local.Pawn = pawn;
			Local.OnNetPossessed();
		}

		Assert.True( pawn.PlayerState.IsValid(), $"Attempted to possess pawn, but pawn '{pawn.DisplayName}' has no attached PlayerState!");
		Viewer = pawn.PlayerState;
	}

	// sync to other clients what this player is currently possessing
	// Sol: when we track observers we could drop this with an Rpc.FilterInclude?
	[Broadcast]
	private void OnNetPossessed()
	{
		if ( IsViewer && IsProxy )
		{
			Possess();
		}
	}

	public void Possess()
	{
		if ( Pawn is null || IsLocalPlayer )
		{
			// Local player - always assume the controller
			PlayerPawn.Possess();
		}
		else
		{
			// A remote player is possessing this player (spectating)
			// So enter the latest known pawn this player has possessed
			Pawn.Possess();
		}
	}

	public void AssignTeam( Team team )
	{
		if ( !Networking.IsHost )
			return;

		Team = team;

		Scene.Dispatch( new TeamAssignedEvent( this, team ) );
	}

	/// <summary>
	/// Called when <see cref="Team"/> changes across the network.
	/// </summary>
	private void OnTeamPropertyChanged( Team before, Team after )
	{
		GameObject.Root.Dispatch( new TeamChangedEvent( before, after ) );

		// Send this to the pawn too if we haveo ne
		if ( PlayerPawn.IsValid() )
		{
			PlayerPawn.GameObject.Root.Dispatch( new TeamChangedEvent( before, after ) );
		}
	}
}
