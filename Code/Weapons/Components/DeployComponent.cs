namespace Facepunch;

public partial class DeployComponent : InputWeaponComponent
{
	[Property] public GameObject PrefabToSpawn { get; set; }

	protected override void OnInputDown()
	{
		if ( IsProxy )
			return;

		var player = Equipment.Owner;
		var tr = Scene.Trace.Ray( new( player.AimRay.Position, player.AimRay.Forward ), 10f )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger" )
			.Run();

		var position = tr.Hit ? tr.HitPosition + tr.Normal * Equipment.Resource.WorldModel.Bounds.Size.Length : player.AimRay.Position + player.AimRay.Forward * 32f;
		var rotation = Rotation.From( 0, player.EyeAngles.yaw, 0f );
		var baseVelocity = player.CharacterController.Velocity;
		var dropped = PrefabToSpawn.Clone( position, rotation );

		var rb = dropped.GetComponentInChildren<Rigidbody>();
		rb.Velocity = baseVelocity + player.AimRay.Forward * 50f + Vector3.Up * 100f;

		dropped.NetworkSpawn();

		// If it's a pawn, possess it
		// This'll probably get removed
		if ( dropped.GetComponent<Pawn>() is Pawn pawn && pawn.IsValid() )
		{
			pawn.Possess();
			DeployedPawnHandler.Create( pawn );
		}

		// Remove self
		player.Inventory.Remove( Equipment.Resource );
	}
}
