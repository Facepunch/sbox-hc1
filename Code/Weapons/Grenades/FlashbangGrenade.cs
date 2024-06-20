namespace Facepunch;

[Title( "Flashbang Grenade" )]
public partial class FlashbangGrenade : BaseGrenade, IMarkerObject
{
	/// <summary>
	/// Where is the marker?
	/// </summary>
	Vector3 IMarkerObject.MarkerPosition => Transform.Position;

	/// <summary>
	/// How far can we see this marker?
	/// </summary>
	float IMarkerObject.MarkerMaxDistance => 512f;

	/// <summary>
	/// What icon?
	/// </summary>
	string IMarkerObject.MarkerIcon => "/ui/weapons/grenades/he_grenade_01.png";

	/// <summary>
	/// What text?
	/// </summary>
	string IMarkerObject.DisplayText => "Flashbang";

	protected override void Explode()
	{
		base.Explode();
	}
}
