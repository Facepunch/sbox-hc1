using Sandbox.Events;

namespace Facepunch;

[Title( "Throw Weapon" ), Group( "Weapon Components" )]
public partial class ThrowWeaponComponent : InputWeaponComponent,
	IGameEventHandler<EquipmentHolsteredEvent>
{
	[Property, EquipmentResourceProperty] public float CookTime { get; set; } = 0.25f;
	[Property, EquipmentResourceProperty] public GameObject Prefab { get; set; }
	[Property] public float ThrowPower { get; set; } = 1200f;

	public enum State
	{
		Idle,
		Cook,
		Throwing,
		Thrown
	}

	[Sync] public State ThrowState { get; private set; }

	private TimeSince TimeSinceAction { get; set; }
	private bool HasThrownOnHost { get; set; }

	void IGameEventHandler<EquipmentHolsteredEvent>.OnGameEvent( EquipmentHolsteredEvent eventArgs )
	{
		if ( IsProxy ) return;
		if ( ThrowState == State.Thrown ) return;
		ThrowState = State.Idle;
	}

	protected bool CanThrow()
	{
		// Player
		if ( Equipment.Owner.IsFrozen )
			return false;

		return true;
	}

	protected override void OnInputDown()
	{
		if ( !CanThrow() )
			return;

		ThrowState = State.Cook;
		TimeSinceAction = 0;
	}

	protected override void OnInputUp()
	{
		if ( !CanThrow() )
		{
			ThrowState = State.Idle;
			TimeSinceAction = 0;

			return;
		}

		if ( TimeSinceAction > CookTime && ThrowState == State.Cook )
		{
			ThrowState = State.Throwing;
			TimeSinceAction = 0;
			return;
		}

		ThrowState = State.Idle;
		TimeSinceAction = 0;
	}

	protected override void OnUpdate()
	{
		if ( Networking.IsHost && HasThrownOnHost && TimeSinceAction > 0.25f )
		{
			// We want to remove the weapon on the host only.
			var player = Equipment.Owner;
			player.Inventory.RemoveWeapon( Equipment );
			return;
		}
		
		if ( IsProxy ) return;

		if ( Input.Pressed( "Attack2" ) )
		{
			ThrowState = State.Idle;
			TimeSinceAction = 0;
		}
		
		if ( ThrowState == State.Throwing && TimeSinceAction > 0.25f )
		{
			Throw();
			ThrowState = State.Thrown;
			TimeSinceAction = 0f;
		}
	}

	[Broadcast]
	protected void Throw()
	{
		var player = Equipment.Owner;
		
		if ( !IsProxy )
		{
			var tr = Scene.Trace.Ray( new( player.AimRay.Position, player.AimRay.Forward ), 10f )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "trigger" )
				.Run();

			var position = tr.Hit ? tr.HitPosition + tr.Normal * Equipment.Resource.WorldModel.Bounds.Size.Length : player.AimRay.Position + player.AimRay.Forward * 32f;
			var rotation = Rotation.From( 0, player.EyeAngles.yaw + 90f, 90f );
			var baseVelocity = player.CharacterController.Velocity;
			var dropped = Prefab.Clone( position, rotation );
			dropped.Tags.Set( "no_player", true );

			var rb = dropped.Components.Get<Rigidbody>( FindMode.EnabledInSelfAndDescendants );
			rb.Velocity = baseVelocity + player.AimRay.Forward * ThrowPower + Vector3.Up * 100f;

			var grenade = dropped.Components.Get<BaseGrenade>();
			if ( grenade.IsValid() )
				grenade.Player = player;

			dropped.Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
			dropped.NetworkSpawn();
		}

		if ( Equipment.Owner.IsValid() )
		{
			Equipment.Owner.BodyRenderer.Set( "b_throw_grenade", true );
		}

		if ( !Networking.IsHost )
			return;

		RadioSounds.Play( player.GameObject.GetTeam(), RadioSound.ThrownGrenade );
		TimeSinceAction = 0f;
		HasThrownOnHost = true;
	}
}
