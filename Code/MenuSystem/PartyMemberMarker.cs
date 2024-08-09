namespace Facepunch;

public partial class PartyMember : Component, IMarkerObject
{
	public Friend? Friend { get; set; }

	public Vector3 MarkerPosition
	{
		get
		{
			if ( !Renderer.IsValid() ) return Vector3.Zero;
			var head = Renderer.GetBoneObject( "Head" );
			if ( !head.IsValid() ) return Vector3.Zero;

			return head.Transform.Position + Vector3.Up * 15f;
		}
	}

	public string DisplayText => Friend.HasValue ? Friend.Value.Name : "";

	/// <summary>
	/// The model renderer.
	/// </summary>
	[Property] public SkinnedModelRenderer Renderer { get; set; }
}
