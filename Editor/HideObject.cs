using Editor;
using Sandbox;

public class HideObject
{
	private static void Toggle()
	{
		foreach ( var item in EditorScene.Selection )
		{
			var thing = item as GameObject;

			thing.Enabled = !thing.Enabled;
		}
	}

	[Menu( "Editor", "Nicked/Toggle Selected" )]
	[Shortcut( "hc1.editor.toggleselect", "H", ShortcutType.Window )]
	static void ToggleSelection()
	{
		using var scope = SceneEditorSession.Scope();

		Toggle();

		SceneEditorSession.Active.AddUndo( "Toggle Selection", Toggle, Toggle );
	}
}
