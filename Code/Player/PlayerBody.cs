﻿namespace Facepunch;
	
public partial class PlayerBody : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public ModelPhysics Physics { get; set; }

	public bool IsRagdoll => Physics.Enabled;

	public Vector3 DamageTakenPosition { get; set; }
	public Vector3 DamageTakenForce { get; set; }

	private bool IsShown;

	public PlayerPawn Player { get; set; }

	protected override void OnStart()
	{
		Player = Components.Get<PlayerPawn>( FindMode.EverythingInSelfAndAncestors );
	}

	internal void SetRagdoll( bool ragdoll )
	{
		Physics.Enabled = ragdoll;
		Renderer.UseAnimGraph = !ragdoll;

		GameObject.Tags.Set( "ragdoll", ragdoll );

		if ( !ragdoll )
		{
			GameObject.Transform.LocalPosition = Vector3.Zero;
			GameObject.Transform.LocalRotation = Rotation.Identity;
		}

		ShowBodyParts( ragdoll );

		if ( ragdoll && DamageTakenForce.LengthSquared > 0f )
			ApplyRagdollImpulses( DamageTakenPosition, DamageTakenForce );

		Transform.ClearInterpolation();
	}

	internal void ApplyRagdollImpulses( Vector3 position, Vector3 force )
	{
		if ( Physics?.PhysicsGroup == null )
			return;

		foreach ( var body in Physics.PhysicsGroup.Bodies )
		{
			body.ApplyImpulseAt( position, force );
		}
	}

	public void ReapplyVisibility()
	{
		ShowBodyParts( IsShown );
	}

	public void ShowBodyParts( bool show )
	{
		// TODO: remove this
		Tags.Set( "invis", !show );

		IsShown = show;

		// Disable the player's body so it doesn't render.
		var skinnedModels = Components.GetAll<ModelRenderer>( FindMode.EnabledInSelfAndDescendants );

		foreach ( var skinnedModel in skinnedModels )
		{
			skinnedModel.RenderType = show ?
				ModelRenderer.ShadowRenderType.On :
				ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
	}
}
