namespace Facepunch;

/// <summary>
/// A health component for any kind of GameObject.
/// </summary>
public partial class HealthComponent : Component, IRespawnable
{
	private float InternalArmor = 0f;
	private float InternalHealth = 100f;
	private bool InternalHasHelmet = false;
	private LifeState InternalState = LifeState.Alive;

	/// <summary>
	/// An action (mainly for ActionGraphs) to respond to when a GameObject's health changes.
	/// </summary>
	[Property] public Action<float, float> OnHealthChanged { get; set; }

	/// <summary>
	/// How long does it take to respawn this object?
	/// </summary>
	public float RespawnTime => 5;

	/// <summary>
	/// How long has it been since life state changed?
	/// </summary>
	private TimeSince TimeSinceLifeStateChanged = 1f;

	/// <summary>
	/// How much to reduce damage by when we have armor.
	/// </summary>
	private float ArmorReduction { get; set; } = 0.775f;
	
	/// <summary>
	/// A list of all Respawnable things on this GameObject
	/// </summary>
	protected IEnumerable<IRespawnable> Respawnables => GameObject.Root.Components.GetAll<IRespawnable>(FindMode.EnabledInSelfAndDescendants);

	/// <summary>
	/// What's our health?
	/// </summary>
	[HostSync( Query = true ), Property, ReadOnly]
	public float Health
	{
		get => InternalHealth;
		set
		{
			var old = InternalHealth;
			if ( old == value ) return;

			InternalHealth = value;
			HealthChanged( old, InternalHealth );
		}
	}

	[HostSync( Query = true ), Property, ReadOnly]
	public float Armor
	{
		get => InternalArmor;
		set
		{
			var old = InternalArmor;
			if ( old == value ) return;
			InternalArmor = value;
		}
	}

	[HostSync( Query = true ), Property, ReadOnly]
	public bool HasHelmet
	{
		get => InternalHasHelmet;
		set
		{
			var old = InternalHasHelmet;
			if ( old == value ) return;
			InternalHasHelmet = value;
		}
	}

	/// <summary>
	/// What's our life state?
	/// </summary>
	[HostSync( Query = true ), Property, ReadOnly, Group( "Life State" )]
	public LifeState State
	{
		get => InternalState;
		set
		{
			var old = InternalState;
			if ( old == value ) return;

			InternalState = value;
			TimeSinceLifeStateChanged = 0;
			LifeStateChanged( old, InternalState );
		}
	}

	/// <summary>
	/// Called when Health is changed.
	/// </summary>
	/// <param name="oldValue"></param>
	/// <param name="newValue"></param>
	protected void HealthChanged( float oldValue, float newValue )
	{
		OnHealthChanged?.Invoke( oldValue, newValue );
	}

	protected void LifeStateChanged( LifeState oldValue, LifeState newValue )
	{
		switch ( newValue )
		{
			case LifeState.Alive:
				Respawnables.ToList().ForEach( x => x.Respawn() );
				break;
			case LifeState.Dead or LifeState.Respawning when oldValue == LifeState.Alive:
				InternalHealth = 0;
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
		Armor = Armor.Clamp( 0, 100 );
		
		return damage * ArmorReduction;
	}

	[Broadcast]
	private void BroadcastKill( Guid killerComponent, Guid victimComponent, float damage, Vector3 position, Vector3 force = default, Guid inflictorComponent = default, bool isHeadshot = false )
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
				isHeadshot );
		}
	}

	[Broadcast]
	public void TakeDamage( float damage, Vector3 position, Vector3 force, Guid attackerId, Guid inflictorId = default, bool isHeadshot = false )
	{
		if ( isHeadshot )
		{
			if ( HasHelmet )
			{
				// Helmet negates headshot damage
				HasHelmet = false;
			}
			else
			{
				damage *= 2f;
			}
		}

		damage = damage.CeilToInt();

		// Only the host should control the damage state
		if ( Networking.IsHost )
		{
			// Let armor try its hand
			damage = CalculateArmorDamage( damage );
			Health -= damage;

			// Did we die?
			if ( Health <= 0 && State == LifeState.Alive )
			{
				State = LifeState.Dead;

				// Broadcast this kill (for feed)
				BroadcastKill( attackerId, Id, damage, position, force, inflictorId, isHeadshot );
			}
		}

		Log.Info( $"{GameObject.Name}.OnDamage( {damage} ): {Health}, {State}" );

		var attackingComponent = Scene.Directory.FindComponentByGuid( attackerId );
		var receivers = GameObject.Root.Components.GetAll<IDamageListener>();
		foreach ( var x in receivers )
		{
			x.OnDamageTaken( damage, position, force, attackingComponent, isHeadshot );
		}

		if ( attackingComponent.IsValid() )
		{
			var givers = attackingComponent.GameObject.Root.Components.GetAll<IDamageListener>();
			foreach ( var x in givers )
			{
				x.OnDamageGiven( damage, position, force, this, isHeadshot );
			}
		}
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;
		
		if ( State == LifeState.Respawning && TimeSinceLifeStateChanged > RespawnTime )
		{
			State = LifeState.Alive;
		}
	}
}


/// <summary>
/// The component's life state.
/// </summary>
public enum LifeState
{
	Alive,
	Respawning,
	Dead
}

/// <summary>
/// A respawnable object.
/// </summary>
public interface IRespawnable
{
	public void Respawn() { }
	public void Kill() { }
}
