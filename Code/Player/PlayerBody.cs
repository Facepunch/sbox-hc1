

namespace Facepunch
{
	public enum BodyRenderMode
	{
		Show,
		Hide,
		ShadowsOnly
	}

	public partial class PlayerBody : Component
	{
		[Property] public SkinnedModelRenderer Renderer { get; set; }
		[Property] public Rigidbody Rigidbody { get; set; }
		[Property] public ModelPhysics Physics { get; set; }

		public bool IsRagdoll => Physics.Enabled;

		internal void SetRagdoll( bool ragdoll )
		{
			Physics.Enabled = ragdoll;
			Rigidbody.Enabled = ragdoll;

			if ( !ragdoll )
			{
				GameObject.Transform.LocalPosition = Vector3.Zero;
				GameObject.Transform.LocalRotation = Rotation.Identity;
			}

			ShowBodyParts( ragdoll ? BodyRenderMode.Show : BodyRenderMode.ShadowsOnly );
		}

		public void ShowBodyParts( BodyRenderMode renderMode )
		{
			// Disable the player's body so it doesn't render.
			var skinnedModels = Components.GetAll<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants );

			foreach ( var skinnedModel in skinnedModels )
			{
				skinnedModel.Enabled = renderMode != BodyRenderMode.Hide;

				if ( skinnedModel.Enabled )
				{
					skinnedModel.RenderType = renderMode == BodyRenderMode.Show ?
						ModelRenderer.ShadowRenderType.On :
						ModelRenderer.ShadowRenderType.ShadowsOnly;
				}
			}
		}
	}
}
