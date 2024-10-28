namespace Facepunch;

public static class Preferences
{
	/// <summary>
	/// Should we show our volumes in the editor?
	/// </summary>
	[ConVar( "hc1_editor_volumes" )]
	public static bool ShowVolumes { get; set; } = true;
}
