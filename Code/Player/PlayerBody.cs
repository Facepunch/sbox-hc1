

namespace Facepunch
{
	public partial class PlayerBody : Component
	{
		[Property] public SkinnedModelRenderer Renderer { get; set; }
		[Property] public Rigidbody Rigidbody { get; set; }
		[Property] public ModelPhysics Physics { get; set; }

		internal void SetRagdoll( bool ragdoll )
		{
			Physics.Enabled = ragdoll;
			Rigidbody.Enabled = ragdoll;

			if ( !ragdoll )
			{
				GameObject.Transform.LocalPosition = Vector3.Zero;
			}

			ShowBodyParts( ragdoll );
		}

		public void ShowBodyParts( bool show )
		{
			// Disable the player's body so it doesn't render.
			var skinnedModels = Components.GetAll<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants );

			foreach ( var skinnedModel in skinnedModels )
			{
				skinnedModel.RenderType = show ? 
					ModelRenderer.ShadowRenderType.On : 
					ModelRenderer.ShadowRenderType.ShadowsOnly;
			}
		}
	}
}
