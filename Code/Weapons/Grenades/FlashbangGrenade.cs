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

	/// <summary>
	/// How long does the effect last?
	/// </summary>
	[Property]
	public float EffectLifeTime { get; set; } = 4f;

	protected override void Explode()
	{
		var effect = Scene.Camera.Components.Create<FlashbangEffect>();
		effect.LifeTime = EffectLifeTime;
		
		base.Explode();
	}
}
