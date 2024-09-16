using Sandbox.Audio;

namespace Facepunch;

public partial class PlayerVoiceComponent : Voice
{
	[Property] public PlayerState PlayerState { get; set; }

	IVoiceFilter Filter { get; set; }

	protected override void OnStart()
	{
		Filter = Scene.GetAllComponents<IVoiceFilter>().FirstOrDefault();
		TargetMixer = Mixer.FindMixerByName( "Voice" );
	}

	protected override IEnumerable<Connection> ExcludeFilter()
	{
		if ( Filter is null ) return base.ExcludeFilter();
		return Filter.GetExcludeFilter();
	}
}
