namespace Facepunch;

/// <summary>
/// A health component for any kind of GameObject.
/// </summary>
public partial class HealthComponent : Component, IRespawnable
{
	/// <summary>
	/// Are we in god mode?
	/// </summary>
	[Property, HostSync] public bool IsGodMode { get; set; } = false;

	/// <summary>
	/// An action (mainly for ActionGraphs) to respond to when a GameObject's health changes.
	/// </summary>
	[Property] public Action<float, float> OnHealthChanged { get; set; }

	/// <summary>
	/// How long has it been since life state changed?
	/// </summary>
	public TimeSince TimeSinceLifeStateChanged { get; private set; } = 1f;

	/// <summary>
	/// Are we ready to respawn?
	/// </summary>
	[HostSync]
	public RespawnState RespawnState { get; set; }

	/// <summary>
	/// How much to reduce damage by when we have armor.
	/// </summary>
	private float ArmorReduction => 0.775f;

	/// <summary>
	/// A list of all Respawnable things on this GameObject
	/// </summary>
	protected IEnumerable<IRespawnable> Respawnables => GameObject.Root.Components.GetAll<IRespawnable>(FindMode.EnabledInSelfAndDescendants);

	/// <summary>
	/// What's our health?
	/// </summary>
	[Property, ReadOnly, HostSync, Change( nameof( OnHealthPropertyChanged ))]
	public float Health { get; set; }

	[Property, ReadOnly, HostSync]
	public float Armor { get; set; }

	public float MaxHealth => GetGlobal<PlayerGlobals>().MaxHealth;
	public float MaxArmor => GetGlobal<PlayerGlobals>().MaxArmor;

	[Property, ReadOnly, HostSync, Change( nameof( OnHasHelmetChanged ) )]
	public bool HasHelmet { get; set; }

	/// <summary>
	/// What's our life state?
	/// </summary>
	[Property, ReadOnly, Group( "Life State" ), HostSync, Change( nameof( OnStatePropertyChanged )) ]
	public LifeState State { get; set; }

	protected void OnHasHelmetChanged( bool oldValue, bool newValue )
	{
		foreach ( var listener in GameObject.Root.Components.GetAll<IArmorListener>() )
		{
			listener?.OnHelmetChanged( newValue );
		}
	}

	/// <summary>
	/// Called when <see cref="Health"/> is changed across the network.
	/// </summary>
	/// <param name="oldValue"></param>
	/// <param name="newValue"></param>
	protected void OnHealthPropertyChanged( float oldValue, float newValue )
	{
		OnHealthChanged?.Invoke( oldValue, newValue );
	}

	protected void OnStatePropertyChanged( LifeState oldValue, LifeState newValue )
	{
		TimeSinceLifeStateChanged = 0f;
		
		switch ( newValue )
		{
			case LifeState.Alive:
				Health = MaxHealth;
				Respawnables.ToList().ForEach( x => x.Respawn() );
				break;
			case LifeState.Dead:
				Health = 0f;
				Respawnables.ToList()
					.ForEach( x => x.Kill() );
				break;
		}
	}
	
	/// <summary>
	/// Calculate how much damage we soak in from armor, and remove armor
	/// </summary>
	/// <param name="damage"></param>
	/// <returns></returns>
	public float CalculateArmorDamage( float damage )
	{
		if ( Armor <= 0 )
			return damage;
		
		Armor -= damage;
		Armor = Armor.Clamp( 0f, 100f );
		
		return damage * GetGlobal<PlayerGlobals>().BaseArmorReduction;
	}

	[DeveloperCommand( "Toggle Kevlar & Helmet" )]
	private static void Dev_ToggleKevlarAndHelmet()
	{
		var player = GameUtils.Viewer;
		if ( player.HealthComponent.HasHelmet )
		{
			player.HealthComponent.Armor = 0;
			player.HealthComponent.HasHelmet = false;
		}
		else
		{
			player.HealthComponent.Armor = player.HealthComponent.MaxArmor;
			player.HealthComponent.HasHelmet = true;
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	private void BroadcastKill( float damage, Vector3 position, Vector3 force, Guid attackerId, Guid inflictorId = default, string hitbox = "", string tags = "" )
	{
		var attacker = Scene.Directory.FindComponentByGuid( attackerId );
		var inflictor = Scene.Directory.FindComponentByGuid( inflictorId );

		var damageEvent = DamageEvent.From( attacker, damage, inflictor, position, force, hitbox, tags ) with { Victim = GameUtils.GetPlayerFromComponent( this ) };

		foreach ( var listener in Scene.GetAllComponents<IKillListener>() )
		{
			listener.OnPlayerKilled( damageEvent );
		}
	}

	private string GetFirstWord( string text )
	{
		var candidate = text.Trim();
		if ( !candidate.Any( char.IsWhiteSpace ) )
			return text;

		return candidate.Split( ' ' ).FirstOrDefault();
	}

	[Broadcast]
	public void TakeDamage( float damage, Vector3 position, Vector3 force, Guid attackerId, Guid inflictorId = default, string hitbox = "", string tags = "" )
	{
		var firstHitbox = GetFirstWord( hitbox );

		var attacker = Scene.Directory.FindComponentByGuid( attackerId );
		var inflictor = Scene.Directory.FindComponentByGuid( inflictorId );

		var damageEvent = DamageEvent.From( attacker, damage, inflictor, position, force, hitbox, tags ) with { Victim = GameUtils.GetPlayerFromComponent( this ) };

		// Edge case, but it's okay.
		if ( firstHitbox == "head" && HasHelmet )
			firstHitbox += " helmet";

		if ( GetGlobal<PlayerGlobals>().GetDamageMultiplier( firstHitbox ) is { } damageMultiplier
			&& !tags.Contains( "melee" ) )
		{
			damageEvent.Damage *= damageMultiplier;
		}

		damageEvent.Damage = damageEvent.Damage.CeilToInt();

		// Only the host should control the damage state
		if ( Networking.IsHost )
		{
			if ( !IsGodMode )
			{
				// Let armor try its hand
				damageEvent.Damage = CalculateArmorDamage( damageEvent.Damage );

				Health -= damageEvent.Damage;
			}

			// Did we die?
			if ( Health <= 0f && State == LifeState.Alive )
			{
				State = LifeState.Dead;

				BroadcastKill( damage, position, force, attackerId, inflictorId, hitbox, tags );
			}
		}

		Log.Info( damageEvent );

		var receivers = GameObject.Root.Components.GetAll<IDamageListener>();
		foreach ( var x in receivers )
		{
			x.OnDamageTaken( damageEvent );
		}

		if ( attacker.IsValid() )
		{
			var givers = attacker.GameObject.Root.Components.GetAll<IDamageListener>();
			foreach ( var x in givers )
			{
				x.OnDamageGiven( damageEvent );
			}
		}

		if ( hitbox.Contains( "head" ) )
		{
			// Helmet negates headshot damage
			if ( HasHelmet )
			{
				if ( !IsGodMode && Networking.IsHost )
				{
					HasHelmet = false;
				}
			}
		}
	}
}


/// <summary>
/// The component's life state.
/// </summary>
public enum LifeState
{
	Alive,
	Dead
}

public enum RespawnState
{
	None,
	CountingDown,
	Ready
}

/// <summary>
/// A respawnable object.
/// </summary>
public interface IRespawnable
{
	public void Respawn() { }
	public void Kill() { }
}

public interface IArmorListener
{
	public void OnHelmetChanged( bool hasHelmet ) { }
	public void OnArmorChanged( float oldValue, float newValue ) { }
}
