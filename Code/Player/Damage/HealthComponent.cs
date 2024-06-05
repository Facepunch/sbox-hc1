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

	[Property, ReadOnly, HostSync]
	public bool HasHelmet { get; set; }

	/// <summary>
	/// What's our life state?
	/// </summary>
	[Property, ReadOnly, Group( "Life State" ), HostSync, Change( nameof( OnStatePropertyChanged )) ]
	public LifeState State { get; set; }

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
				Health = 100f;
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
		
		return damage * ArmorReduction;
	}

	[Broadcast]
	private void BroadcastKill( Guid killerComponent, Guid victimComponent, float damage, Vector3 position, Vector3 force = default, Guid inflictorComponent = default, string hitbox = "" )
	{
		foreach ( var listener in Scene.GetAllComponents<IKillListener>() )
		{
			listener.OnPlayerKilled(
				Scene.Directory.FindComponentByGuid( killerComponent ),
				Scene.Directory.FindComponentByGuid( victimComponent ),
				damage, 
				position,
				force,
				Scene.Directory.FindComponentByGuid( inflictorComponent ),
				hitbox );
		}
	}

	[Property] public float HeadshotMultiplier { get; set; } = 2f;

	[Broadcast]
	public void TakeDamage( float damage, Vector3 position, Vector3 force, Guid attackerId, Guid inflictorId = default, string hitbox = "" )
	{
		if ( hitbox.Contains( "head" ) )
		{
			// Helmet negates headshot damage
			if ( HasHelmet )
				HasHelmet = false;
			else
				damage *= HeadshotMultiplier;
		}

		damage = damage.CeilToInt();

		// Only the host should control the damage state
		if ( Networking.IsHost )
		{
			if ( !IsGodMode )
			{
				// Let armor try its hand
				damage = CalculateArmorDamage( damage );

				Health -= damage;
			}

			// Did we die?
			if ( Health <= 0f && State == LifeState.Alive )
			{
				State = LifeState.Dead;

				// Broadcast this kill (for feed)
				BroadcastKill( attackerId, Id, damage, position, force, inflictorId, hitbox );
			}
		}

		Log.Info( $"{GameObject.Name}.OnDamage( {damage} ): {Health}, {State}" );

		var attackingComponent = Scene.Directory.FindComponentByGuid( attackerId );
		var receivers = GameObject.Root.Components.GetAll<IDamageListener>();
		foreach ( var x in receivers )
		{
			x.OnDamageTaken( damage, position, force, attackingComponent, hitbox );
		}

		if ( attackingComponent.IsValid() )
		{
			var givers = attackingComponent.GameObject.Root.Components.GetAll<IDamageListener>();
			foreach ( var x in givers )
			{
				x.OnDamageGiven( damage, position, force, this, hitbox );
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
