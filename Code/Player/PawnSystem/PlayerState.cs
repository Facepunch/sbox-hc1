﻿using Sandbox.Diagnostics;
using Sandbox.Events;
using System.Runtime.Versioning;
using System.Text.Json.Serialization;

namespace Facepunch;

public partial class PlayerState : Component
{
	/// <summary>
	/// The Player we're currently in the view of (clientside)
	/// </summary>
	public static PlayerState CurrentPlayerState => GameUtils.LocalPlayerState;
	
	/// <summary>
	/// The prefab to spawn when we want to make a player pawn for the player.
	/// </summary>
	[Property] public GameObject PlayerPawnPrefab { get; set; }

	/// <summary>
	/// Sync this so other people know what player belongs to who
	/// </summary>
	[Sync] private Guid currentPlayerPawnGuid { get; set; }

	/// <summary>
	/// We always want a reference to this player state's current player pawn. 
	/// They could have this AND be controlling a drone for example.
	/// </summary>
	public PlayerPawn CurrentPlayerPawn
	{
		get => Scene.Directory.FindComponentByGuid( currentPlayerPawnGuid ) as PlayerPawn;
		set => currentPlayerPawnGuid = value.Id;
	}

	[Broadcast]
	public void RequestCreatePlayer()
	{
		if ( CurrentPlayerPawn.IsValid() )
			return;

		CreatePlayer();
	}

	public void CreatePlayer()
	{
		var prefab = PlayerPawnPrefab.Clone();
		var pawn = prefab.Components.Get<PlayerPawn>();
		pawn.PlayerState = this;
		prefab.NetworkSpawn( Network.OwnerConnection );

		CurrentPlayerPawn = pawn;

		// Notify
		NotifyPossessed( pawn.Id );

		GameMode.Instance?.SendSpawnConfirmation( pawn.Id );

		pawn.Respawn();
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
		if ( Pawn.IsValid() )
		{
			Pawn.GameObject.Root.Dispatch( new TeamChangedEvent( before, after ) );
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
	public bool IsViewer => CurrentPlayerState == this;

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
		if ( !IsProxy && !IsBot && !GameUtils.LocalPlayerState.IsValid() )
		{
			GameUtils.LocalPlayerState = this;
		}
	}

	/// <summary>
	/// Called from client when we've taken possession of a pawn.
	/// </summary>
	public void NotifyPossessed( Pawn pawn )
	{
		Assert.True( !IsProxy );
		NotifyPossessed( pawn.Id );
	}

	[Broadcast]
	private void NotifyPossessed( Guid guid )
	{
		if ( Networking.IsHost )
			pawnGuid = guid;

		// todo: this should be an engine feature?
		Pawn = Scene.Directory.FindComponentByGuid( guid ) as Pawn;

		if ( IsViewer && IsProxy )
		{
			Possess( Pawn );
		}
	}

	public void Possess( Pawn pawn )
	{
		Assert.True( pawn.IsValid(), "PlayerState attempted Possess but has no Controller!" );

		pawn.OnPossess();
		Pawn = pawn;
	}

	public void DePossess()
	{
		if ( Pawn.IsValid() )
		{
			Pawn.OnDePossess();
			Pawn = null;
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