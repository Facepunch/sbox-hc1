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
	
	/// <summary>
	/// The sound effect to use while the flashbang effect is active.
	/// </summary>
	[Property]
	public SoundEvent SoundEffect { get; set; }

	protected override void Explode()
	{
		base.Explode();
		
		var viewer = GameUtils.Viewer?.Controller;
		if ( !viewer.IsValid() ) return;

		var distance = viewer.Transform.Position.Distance( Transform.Position );
		if ( distance > 750f ) return;

		var trace = Scene.Trace
			.WithoutTags( "trigger", "invis", "ragdoll" )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.UseHitboxes()
			.Ray( Transform.Position + new Vector3( 0f, 0f, 8f ), viewer.Head.Transform.Position )
			.Run();

		if ( !trace.GameObject.IsValid() )
			return;
		
		if ( trace.GameObject.Root != viewer.GameObject.Root )
			return;

		var direction = (Transform.Position - Scene.Camera.Transform.Position).Normal;
		var dot = direction.Dot( Scene.Camera.Transform.Rotation.Forward );
		if ( dot < 0.35f ) return;
		
		var effect = Scene.Camera.Components.Create<FlashbangEffect>();
		effect.SoundEffect = SoundEffect;
		effect.LifeTime = EffectLifeTime;
	}
}
