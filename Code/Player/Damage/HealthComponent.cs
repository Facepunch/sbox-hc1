using Facepunch.UI;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Events;

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
	/// A list of all Respawnable things on this GameObject
	/// </summary>
	protected IEnumerable<IRespawnable> Respawnables => GameObject.Root.Components.GetAll<IRespawnable>(FindMode.EnabledInSelfAndDescendants);

	/// <summary>
	/// What's our health?
	/// </summary>
	[HostSync, Change( nameof( OnHealthPropertyChanged ))]
	public float Health { get; set; }

	public float MaxHealth => GetGlobal<PlayerGlobals>().MaxHealth;

	/// <summary>
	/// What's our life state?
	/// </summary>
	[Group( "Life State" ), HostSync, Change( nameof( OnStatePropertyChanged ) )]
	public LifeState State { get; set; } = LifeState.Dead;

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

	public void TakeDamage( DamageInfo damageInfo )
	{
		Assert.True( Networking.IsHost );

		damageInfo = WithThisAsVictim( damageInfo );
		damageInfo = ModifyDamage( damageInfo );

		BroadcastDamage( damageInfo );

		if ( IsGodMode ) return;

		Health = Math.Max( 0f, Health - damageInfo.Damage );

		if ( Health > 0f || State != LifeState.Alive ) return;

		State = LifeState.Dead;

		BroadcastKill( damageInfo );
	}

	private DamageInfo WithThisAsVictim( DamageInfo damageInfo )
	{
		var extraFlags = DamageFlags.None;
		var hitbox = damageInfo.Hitbox;

		if ( damageInfo.WasExplosion || damageInfo.WasMelee ) hitbox = HitboxTags.UpperBody;
		if ( damageInfo.WasFallDamage ) hitbox = HitboxTags.Leg;

		return damageInfo with
		{
			Victim = this,
			Hitbox = hitbox,
			Flags = damageInfo.Flags | extraFlags
		};
	}

	private DamageInfo ModifyDamage( DamageInfo damageInfo )
	{
		var modifyEvent = new ModifyDamageEvent( damageInfo );

		Scene.Dispatch( modifyEvent );

		return modifyEvent.DamageInfo;
	}

	private void BroadcastDamage( DamageInfo damageInfo )
	{
		Log.Info( damageInfo );

		BroadcastDamage( damageInfo.Damage, damageInfo.Position, damageInfo.Force,
			damageInfo.Attacker?.Id ?? default, damageInfo.Inflictor?.Id ?? default,
			damageInfo.Hitbox, damageInfo.Flags );
	}

	private void BroadcastKill( DamageInfo damageInfo )
	{
		BroadcastKill( damageInfo.Damage, damageInfo.Position, damageInfo.Force,
			damageInfo.Attacker?.Id ?? default, damageInfo.Inflictor?.Id ?? default,
			damageInfo.Hitbox, damageInfo.Flags );
	}

	[Broadcast( NetPermission.HostOnly )]
	private void BroadcastDamage( float damage, Vector3 position, Vector3 force, Guid attackerId, Guid inflictorId = default, HitboxTags hitbox = default, DamageFlags flags = default )
	{
		var attacker = Scene.Directory.FindComponentByGuid( attackerId );
		var inflictor = Scene.Directory.FindComponentByGuid( inflictorId );

		var damageInfo = new DamageInfo( attacker, damage, inflictor, position, force, hitbox, flags )
		{
			Victim = GameUtils.GetPlayerFromComponent( this )
		};

		GameObject.Root.Dispatch( new DamageTakenEvent( damageInfo ) );

		if ( damageInfo.Attacker.IsValid() )
		{
			damageInfo.Attacker.GameObject.Root.Dispatch( new DamageGivenEvent( damageInfo ) );
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	private void BroadcastKill( float damage, Vector3 position, Vector3 force, Guid attackerId, Guid inflictorId = default, HitboxTags hitbox = default, DamageFlags flags = default )
	{
		var attacker = Scene.Directory.FindComponentByGuid( attackerId );
		var inflictor = Scene.Directory.FindComponentByGuid( inflictorId );

		var damageInfo = new DamageInfo( attacker, damage, inflictor, position, force, hitbox, flags )
		{
			Victim = this
		};

		Scene.Dispatch( new KillEvent( damageInfo ) );
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
