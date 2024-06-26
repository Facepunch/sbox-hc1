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
	/// We always want a reference to this player state's current player pawn. 
	/// They could have this AND be controlling a drone for example.
	/// </summary>
	public PlayerPawn PlayerPawn
	{
		get => Scene.Directory.FindComponentByGuid( playerPawnGuid ) as PlayerPawn;
		set => playerPawnGuid = value.Id;
	}
	[HostSync] private Guid playerPawnGuid { get; set; } // Sync this so other people know what player belongs to who

	public void Init( Connection connection )
	{
		OnNetInit();

		SteamId = connection.SteamId;
	}

	[Authority]
	private void OnNetInit()
	{
		if ( IsBot )
			return;

		Local = this;
		Log.Info( "Local set!" );
	}	

	public void Kick()
	{
		if ( PlayerPawn.IsValid() )
		{
			PlayerPawn.GameObject.Destroy();
		}

		// todo: actually kick em
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
		Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
	}

	public static void OnPossess( Pawn pawn )
	{
		if ( !pawn.IsProxy )
		{
			Local.NotifyPossessed( pawn.Id );
		}

		Assert.True( pawn.PlayerState.IsValid(), $"Attempted to possess pawn, but pawn '{pawn.DisplayName}' has no attached PlayerState!");
		Viewer = pawn.PlayerState;
	}

	// sync to other clients what this player is currently possessing
	[Broadcast]
	private void NotifyPossessed( Guid guid )
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
}
