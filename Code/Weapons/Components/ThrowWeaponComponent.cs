namespace Facepunch;

public partial class ThrowWeaponComponent : InputWeaponComponent
{
	[Property] public float CookTime { get; set; } = 1f;
	[Property] public GameObject Prefab { get; set; }
	[Property] public float ThrowPower { get; set; } = 1200f;

	public enum State
	{
		Idle,
		Cook,
		Throwing,
		Thrown
	}
	
	[Sync] public State ThrowState { get; set; }

	private TimeSince TimeSinceAction { get; set; }
	private bool HasThrownOnHost { get; set; }
	
	protected override void OnInput()
	{
		if ( ThrowState != State.Idle ) return;

		ThrowState = State.Cook;
		TimeSinceAction = 0f;
	}

	protected override void OnUpdate()
	{
		if ( Networking.IsHost && HasThrownOnHost && TimeSinceAction > 0.25f )
		{
			// We want to remove the weapon on the host only.
			var player = Weapon.PlayerController;
			player.Inventory.RemoveWeapon( Weapon );
			return;
		}
		
		if ( IsProxy ) return;
		if ( ThrowState == State.Idle ) return;

		// Simple state machine to handle this stuff.
		if ( ThrowState == State.Cook && TimeSinceAction > CookTime )
		{
			ThrowState = State.Throwing;
			TimeSinceAction = 0f;
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
		if ( !Networking.IsHost ) return;

		var player = Weapon.PlayerController;
		
		RadioSounds.Play( player.GameObject.GetTeam(), RadioSound.ThrownGrenade );

		var tr = Scene.Trace.Ray( new Ray( player.AimRay.Position, player.AimRay.Forward ), 128 )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger" )
			.Run();

		var position = tr.Hit ? tr.HitPosition + tr.Normal * Weapon.Resource.WorldModel.Bounds.Size.Length : player.AimRay.Position + player.AimRay.Forward * 32f;
		var rotation = Rotation.From( 0, player.EyeAngles.yaw + 90, 90 );
		var baseVelocity = player.CharacterController.Velocity;
		var dropped = Prefab.Clone( position, rotation );

		if ( !tr.Hit )
		{
			var rb = dropped.Components.Get<Rigidbody>( FindMode.EnabledInSelfAndDescendants );
			rb.Velocity = baseVelocity + player.AimRay.Forward * ThrowPower + Vector3.Up * 100;
			rb.AngularVelocity = Vector3.Random * 8.0f;
		}

		var grenade = dropped.Components.Get<BaseGrenade>();
		if ( grenade.IsValid() )
			grenade.ThrowerId = player.Id;

		dropped.NetworkSpawn();

		TimeSinceAction = 0f;
		HasThrownOnHost = true;
	}
}
