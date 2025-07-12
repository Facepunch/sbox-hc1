namespace Facepunch;

public partial class PlayerBody : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public ModelPhysics Physics { get; set; }
	[Property] public PlayerPawn Player { get; set; }
	[Property] public GameObject FirstPersonBody { get; set; }
	[Property] public List<AnimationHelper> AnimationHelpers { get; set; } = new();

	public Vector3 DamageTakenPosition { get; set; }
	public Vector3 DamageTakenForce { get; set; }

	private bool IsFirstPerson;
	public bool IsRagdoll => Physics.Enabled;

	internal void SetRagdoll( bool ragdoll )
	{
		Physics.Enabled = ragdoll;
		Renderer.UseAnimGraph = !ragdoll;

		GameObject.Tags.Set( "ragdoll", ragdoll );

		if ( !ragdoll )
		{
			GameObject.LocalPosition = Vector3.Zero;
			GameObject.LocalRotation = Rotation.Identity;
		}

		SetFirstPersonView( !ragdoll );

		if ( ragdoll && DamageTakenForce.LengthSquared > 0f )
			ApplyRagdollImpulses( DamageTakenPosition, DamageTakenForce );

		Transform.ClearInterpolation();
	}

	internal void ApplyRagdollImpulses( Vector3 position, Vector3 force )
	{
		if ( !Physics.IsValid() || !Physics.PhysicsGroup.IsValid() )
			return;

		foreach ( var body in Physics.PhysicsGroup.Bodies )
		{
			body.ApplyImpulseAt( position, force );
		}
	}

	public void Refresh()
	{
		SetFirstPersonView( IsFirstPerson );
	}

	public void SetFirstPersonView( bool firstPerson )
	{
		IsFirstPerson = firstPerson;

		if ( Player.CurrentEquipment.IsValid() )
		{
			Player.CurrentEquipment.UpdateRenderMode();
		}

		FirstPersonBody.Enabled = IsFirstPerson;
	}

	protected override void OnUpdate()
	{
		if ( !Player.IsValid() )
			return;

		if ( !Player.CameraController.IsValid() )
			return;

		var isWatchingThisPlayer = Client.Viewer.IsValid() && Client.Viewer.Pawn == Player;
		Tags.Set( "viewer", isWatchingThisPlayer && Player.CameraController.Mode == CameraMode.FirstPerson );
	}

	internal void UpdateRotation( Rotation rotation )
	{
		WorldRotation = rotation;
		FirstPersonBody.WorldRotation = rotation;
	}
}
