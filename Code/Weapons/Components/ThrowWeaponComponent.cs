namespace Facepunch;

public partial class ThrowWeaponComponent : InputWeaponComponent, Weapon.IDeploymentListener
{
	[Property] public float CookTime { get; set; } = 0.5f;
	[Property] public GameObject Prefab { get; set; }
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

	void Weapon.IDeploymentListener.OnHolstered( Weapon weapon )
	{
		if ( IsProxy ) return;
		if ( ThrowState == State.Thrown ) return;
		ThrowState = State.Idle;
	}

	protected override void OnInputDown()
	{
		ThrowState = State.Cook;
		TimeSinceAction = 0;
	}

	protected override void OnInputUp()
	{
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
			var player = Weapon.PlayerController;
			player.Inventory.RemoveWeapon( Weapon );
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
		var player = Weapon.PlayerController;
		
		if ( !IsProxy )
		{
			var tr = Scene.Trace.Ray( new( player.AimRay.Position, player.AimRay.Forward ), 128f )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "trigger" )
				.Run();

			var position = tr.Hit ? tr.HitPosition + tr.Normal * Weapon.Resource.WorldModel.Bounds.Size.Length : player.AimRay.Position + player.AimRay.Forward * 32f;
			var rotation = Rotation.From( 0, player.EyeAngles.yaw + 90f, 90f );
			var baseVelocity = player.CharacterController.Velocity;
			var dropped = Prefab.Clone( position, rotation );
			dropped.Tags.Set( "no_player", true );

			if ( !tr.Hit )
			{
				var rb = dropped.Components.Get<Rigidbody>( FindMode.EnabledInSelfAndDescendants );
				rb.Velocity = baseVelocity + player.AimRay.Forward * ThrowPower + Vector3.Up * 100f;
				rb.AngularVelocity = Vector3.Random * 8f;
			}

			var grenade = dropped.Components.Get<BaseGrenade>();
			if ( grenade.IsValid() )
				grenade.ThrowerId = player.Id;

			dropped.Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
			dropped.NetworkSpawn();
		}

		if ( !Networking.IsHost )
			return;

		RadioSounds.Play( player.GameObject.GetTeam(), RadioSound.ThrownGrenade );
		TimeSinceAction = 0f;
		HasThrownOnHost = true;
	}
}
