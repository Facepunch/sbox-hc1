﻿namespace Facepunch;
	
public partial class PlayerBody : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public ModelPhysics Physics { get; set; }
	[Property] public PlayerPawn Player { get; set; }

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

		// we don't want to see our own body in FP view, just the shadow
		Renderer.RenderType = firstPerson ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;

		if ( Player.CurrentEquipment.IsValid() )
		{
			Player.CurrentEquipment.UpdateRenderMode();
		}
	}
}
