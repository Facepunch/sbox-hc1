namespace Facepunch;

public partial class PartyMember : Component, IMarkerObject
{
	public Friend? Friend { get; set; }
	public Vector3 MarkerPosition => Renderer.IsValid() ? Renderer.GetBoneObject( "Head" ).Transform.Position + Vector3.Up * 15 : Vector3.Zero;
	public string DisplayText => Friend.HasValue ? Friend.Value.Name : "";

	/// <summary>
	/// The model renderer.
	/// </summary>
	[Property] public SkinnedModelRenderer Renderer { get; set; }
}
