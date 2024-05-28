namespace Facepunch;

/// <summary>
/// A health component for any kind of GameObject.
/// </summary>
public partial class HealthComponent : Component, IRespawnable
{
	private float health = 100f;
	private LifeState state = LifeState.Alive;

	/// <summary>
	/// An action (mainly for ActionGraphs) to respond to when a GameObject's health changes.
	/// </summary>
	[Property] public Action<float, float> OnHealthChanged { get; set; }

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
	[Sync( Query = true ), Property, ReadOnly]
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
	[Sync( Query = true ), Property, ReadOnly, Group( "Life State" )]
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

	[Broadcast]
	public void TakeDamage( float damage, Vector3 position, Vector3 force, Guid attackerId )
	{
		Health -= damage;

		Log.Info($"{GameObject.Name}.OnDamage( {damage} ): {Health}, {State}");

		if ( Health <= 0 )
		{
			Killed( damage, position, force, attackerId );
		}
	}

	protected void Killed(float damage, Vector3 position, Vector3 force, Guid attackerId)
	{
		LifeState newState = LifeState.Dead;
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
