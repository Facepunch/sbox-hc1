namespace Facepunch;

public partial class DialogModal
{
	public static void Show( string text, string submitText = "OK", string cancelText = "Cancel", Action submitAction = null, Action cancelAction = null )
	{
		// Look for a HUD
		var screenPanel = Game.ActiveScene.GetAllComponents<DialogRoot>().FirstOrDefault();
		if ( !screenPanel.IsValid() )
			return;

		// Create a modal
		var pnl = screenPanel.
			Panel.AddChild<DialogModal>();

		pnl.Text = text;


		//
		pnl.CancelText = cancelText;
		pnl.OnCancel = cancelAction;

		//
		pnl.OnSubmit = submitAction;
		pnl.SubmitText = submitText;
	}
}
