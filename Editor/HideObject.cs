using Editor;
using Sandbox;

public class HideObject
{
	[Menu("Editor", "Settings/HC1/ToggleSelection", Shortcut = "h" )]
	static void ToggleSelection()
	{
		using var scope = SceneEditorSession.Scope();

		var selection = EditorScene.Selection;

		foreach ( var item in EditorScene.Selection )
		{
			var thing = item as GameObject;

			thing.Enabled = !thing.Enabled;
		}

	}
}
