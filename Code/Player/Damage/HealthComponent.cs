namespace Gunfight;

/// <summary>
/// A health component for any kind of GameObject.
/// </summary>
public partial class HealthComponent : Component, Component.IDamageable, IRespawnable
{
	private float health = 100f;
	private LifeState state = LifeState.Alive;

	/// <summary>
	/// An action (mainly for ActionGraphs) to respond to when a GameObject's health changes.
	/// </summary>
	[Property] public Action<float, float> OnHealthChanged { get; set; }

	/// <summary>
	/// An action to respond to when a GameObject's life state changes.
	/// </summary>
	[Property] public Action<LifeState, LifeState> OnLifeStateChanged { get; set; }

	/// <summary>
	/// Called when dead.
	/// </summary>
	[Property] public Func<Sandbox.DamageInfo, LifeState?> OnKilled { get; set; }

	/// <summary>
	/// How long does it take to respawn this object?
	/// </summary>
	[Property] public float RespawnTime { get; set; } = 2f;

	/// <summary>
	/// How long has it been since life state changed?
	/// </summary>
	TimeSince TimeSinceLifeStateChanged = 1;

	/// <summary>
	/// What's our health?
	/// </summary>
	[Property, ReadOnly]
	public float Health
	{
		get => health;
		set
		{
			var old = health;
			if ( old == value ) return;

			health = value;
			HealthChanged( old, health );
		}
	}

	/// <summary>
	/// What's our life state?
	/// </summary>
	[Property, ReadOnly, Group( "Life State" )]
	public LifeState State
	{
		get => state;
		set
		{
			var old = state;
			if ( old == value ) return;

			state = value;
			TimeSinceLifeStateChanged = 0;
			LifeStateChanged( old, state );
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

	protected IEnumerable<IRespawnable> Respawnables => GameObject.Root.Components.GetAll<IRespawnable>( FindMode.EnabledInSelfAndDescendants );

	protected void LifeStateChanged( LifeState oldValue, LifeState newValue )
	{
		OnLifeStateChanged?.Invoke( oldValue, newValue );

		if ( newValue == LifeState.Alive )
		{
			Respawnables.ToList()
				.ForEach( x => x.Respawn() );
		}
		if ( ( newValue == LifeState.Dead || newValue == LifeState.Respawning ) && oldValue == LifeState.Alive )
		{
			Respawnables.ToList()
				.ForEach( x => x.Kill() );
		}
	}

	/// <summary>
	/// Called when this GameObject is damaged by something/someone.
	/// </summary>
	/// <param name="info"></param>
	public void OnDamage( in Sandbox.DamageInfo info )
	{
		Health -= info.Damage;
		info.Damage = Health;

		if ( Health <= 0 )
		{
			Killed( info );
		}
	}

	protected void Killed( Sandbox.DamageInfo info )
	{
		LifeState newState = LifeState.Dead;

		var returnedState = OnKilled?.Invoke( info );
		if ( returnedState.HasValue )
			newState = returnedState.Value;

		State = newState;
	}

	protected override void OnUpdate()
	{
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
