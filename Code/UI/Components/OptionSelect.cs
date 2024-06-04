namespace Facepunch.UI;

[Alias( "optionselect" )]
public class OptionSelect : DropDown
{
	/// <summary>
	/// Open the dropdown.
	/// </summary>
	public override void Open()
	{
		base.Open();

		// Hacky hack
		Popup.StyleSheet.Load( "/UI/Styles/Theme.scss" );
		Popup.AddClass( "hc1" );
	}
}
