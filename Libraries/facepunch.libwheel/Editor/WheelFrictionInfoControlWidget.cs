using Editor;
using Sandbox;

[CustomEditor( typeof( WheelFrictionInfo ) )]
public class WheelFrictionInfoControlWidget : ControlWidget
{
	public override bool SupportsMultiEdit => true;

	public WheelFrictionInfoControlWidget( SerializedProperty property ) : base( property )
	{
		if ( !property.TryGetAsObject( out var obj ) )
		{
			Log.Error( "Couldn't get Wheel Friction Info" );
		}

		Layout = Layout.Column();
		Layout.Margin = 4f;

		Layout.Add( new Label( "Extremum" ) );
		var extremumRow = Layout.AddRow();
		extremumRow.Add( new FloatControlWidget( obj.GetProperty( "ExtremumSlip" ) ) { Label = "", Icon = "east", ToolTip = "Slip", HighlightColor = Theme.Red }, 1 );
		extremumRow.Add( new FloatControlWidget( obj.GetProperty( "ExtremumValue" ) ) { Label = "",Icon = "functions", ToolTip = "Value", HighlightColor = Theme.Red }, 1 );

		Layout.Add( new Separator( 10 ) );

		Layout.Add( new Label( "Asymptote" ) );
		var asymptoteRow = Layout.AddRow();
		asymptoteRow.Add( new FloatControlWidget( obj.GetProperty( "AsymptoteSlip" ) ) { Label = "", Icon = "east", ToolTip = "Slip", HighlightColor = Theme.Blue }, 1 );
		asymptoteRow.Add( new FloatControlWidget( obj.GetProperty( "AsymptoteValue" ) ) { Label = "",Icon = "functions", ToolTip = "Value", HighlightColor = Theme.Blue }, 1 );

		Layout.Add( new Separator( 10 ) );

		Layout.Add( new Label( "Stiffness" ) );
		Layout.Add( new FloatControlWidget( obj.GetProperty( "Stiffness" ) ) { Label = "", Icon = "vertical_align_center", ToolTip = "Stiffness", HighlightColor = Theme.Green }, 1 );
	}

	protected override void PaintUnder()
	{
		Paint.ClearPen();

		Rect localRect = LocalRect;
		Paint.SetBrush( ControlColor.Lighten( 0.25f ) );
		Paint.DrawRect( in localRect, ControlRadius );
	}
}
