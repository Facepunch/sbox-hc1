namespace Facepunch;

public partial class PlayerVoiceComponent : Voice
{
	IVoiceFilter Filter { get; set; }

	protected override void OnStart()
	{
		Filter = Scene.GetAllComponents<IVoiceFilter>().FirstOrDefault();
	}

	protected override bool ShouldExclude( Connection listener )
	{
		if ( Filter is null ) return false;

		return Filter.ShouldExclude( listener );
	}
}
