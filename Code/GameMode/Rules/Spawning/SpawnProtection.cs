﻿using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Dispatched on the host when a player starts being spawn protected.
/// </summary>
public record SpawnProtectionStart( PlayerPawn Player ) : IGameEvent;

/// <summary>
/// Dispatched on the host when a player stops being spawn protected.
/// </summary>
public record SpawnProtectionEnd( PlayerPawn Player ) : IGameEvent;

/// <summary>
/// Makes respawned players invulnerable for a given duration, or until they move / shoot.
/// </summary>
public sealed class SpawnProtection : Component,
	IGameEventHandler<PlayerSpawnedEvent>
{
	private readonly Dictionary<PlayerPawn, TimeSince> _spawnProtectedSince = new();

	[Property, HostSync]
	public float MaxDurationSeconds { get; set; } = 10f;

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent(PlayerSpawnedEvent eventArgs )
	{
		Enable( eventArgs.Player );
	}

	public void DisableAll()
	{
		foreach ( var (player, _) in _spawnProtectedSince.ToArray() )
		{
			Disable( player );
		}
	}

	protected override void OnDisabled()
	{
		DisableAll();
	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost || _spawnProtectedSince.Count == 0 )
		{
			return;
		}

		foreach ( var (player, since) in _spawnProtectedSince.ToArray() )
		{
			if ( !player.IsValid || since > MaxDurationSeconds || player.TimeSinceLastInput < since + 0.1f )
			{
				Disable( player );
			}
		}
	}

	public void Enable( PlayerPawn player )
	{
		_spawnProtectedSince[player] = 0f;

		player.HealthComponent.IsGodMode = true;

		using ( Rpc.FilterInclude( player.Network.OwnerConnection ) )
		{
			GameMode.Instance.ShowToast( "Spawn Protected", duration: MaxDurationSeconds );
		}

		Scene.Dispatch( new SpawnProtectionStart( player ) );
	}

	public void Disable( PlayerPawn player )
	{
		_spawnProtectedSince.Remove( player );

		player.HealthComponent.IsGodMode = false;

		using ( Rpc.FilterInclude( player.Network.OwnerConnection ) )
		{
			GameMode.Instance.HideToast();
		}

		Scene.Dispatch( new SpawnProtectionEnd( player ) );
	}
}
