using Editor;

namespace Facepunch.Editor;

public static class Preferences
{
	[Menu( "Editor", "Nicked/Editor/Toggle Volume Visibility" )]
	public static void OpenMyMenu()
	{
		// Invert this
		Facepunch.Preferences.ShowVolumes ^= true;
	}
}
