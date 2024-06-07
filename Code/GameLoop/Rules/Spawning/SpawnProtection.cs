using Facepunch;

/// <summary>
/// Makes respawned players invulnerable for a given duration, or until they move / shoot.
/// </summary>
public sealed class SpawnProtection : Component, IPlayerSpawnListener
{
	private readonly Dictionary<PlayerController, TimeSince> _spawnProtectedSince = new();

	[Property, HostSync]
	public float MaxDurationSeconds { get; set; } = 10f;

	void IPlayerSpawnListener.PostPlayerSpawn( PlayerController player )
	{
		Enable( player );
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

	public void Enable( PlayerController player )
	{
		_spawnProtectedSince[player] = 0f;

		player.HealthComponent.IsGodMode = true;

		using ( Rpc.FilterInclude( player.Network.OwnerConnection ) )
		{
			GameMode.Instance.ShowToast( "Spawn Protected", duration: MaxDurationSeconds );
		}
	}

	public void Disable( PlayerController player )
	{
		_spawnProtectedSince.Remove( player );

		player.HealthComponent.IsGodMode = false;

		using ( Rpc.FilterInclude( player.Network.OwnerConnection ) )
		{
			GameMode.Instance.HideToast();
		}
	}
}
