namespace Facepunch;

public partial class PlayerVoiceComponent : Voice
{
	IVoiceFilter Filter { get; set; }

	protected override void OnStart()
	{
		Filter = Scene.GetAllComponents<IVoiceFilter>().FirstOrDefault();
	}

	protected override IEnumerable<Connection> ExcludeFilter()
	{
		if ( Filter is null ) return base.ExcludeFilter();
		return Filter.GetExcludeFilter();
	}
}
