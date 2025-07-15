using Editor;
using Facepunch;
using Sandbox;
using System.Collections;
using System.Linq;

[CustomEditor( typeof( BotContext ) )]
public class BotContextControlWidget : ControlWidget
{
	private BotContext _context;
	private Label _behaviorLabel;
	private Label _nodeLabel;
	private GridLayout _headerGrid;
	private GridLayout _contentGrid;

	public BotContextControlWidget( SerializedProperty property ) : base( property )
	{
		Layout = Layout.Column();
		Layout.Spacing = 4;
		Layout.Margin = new( 8, 0, 8, 8 );
		Layout.AddSeparator();

		// Blackboard section
		var header = Layout.AddRow();
		header.Add( new Label.Subtitle( "Data" ) );

		var refresh = header.Add( new IconButton( "refresh" ) );
		refresh.OnClick = () => Rebuild();

		// Header Grid
		_headerGrid = Layout.Grid();
		_headerGrid.HorizontalSpacing = 8;
		_headerGrid.VerticalSpacing = 4;
		_headerGrid.AddCell( 0, 0, new Label.Small( "Key" ) );
		_headerGrid.AddCell( 1, 0, new Label.Small( "Value" ) );

		// Content Grid
		_contentGrid = Layout.Grid();
		_contentGrid.HorizontalSpacing = 8;
		_contentGrid.VerticalSpacing = 4;

		Layout.Add( _headerGrid );
		Layout.Add( _contentGrid );

		Rebuild();
	}

	/// <summary>
	/// A dumb, crude ToString method that wraps stuff for us to display in the grid.
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	private string ToString( object obj )
	{
		return obj switch
		{
			IList l => $"[{l.Count}][{string.Join( ", ", l.Cast<object>().Select( ToString ) )}]",
			PlayerPawn p => p.DisplayName ?? "null",
			string str => str,
			int i => i.ToString(),
			float f => f.ToString( "F2" ),
			bool b => b.ToString(),
			Vector3 v => v.ToString(),
			Component c => $"{c.GameObject.ToString()}",
			_ => obj?.ToString() ?? "null"
		};
	}

	private void Rebuild()
	{
		_contentGrid.Clear( true );

		_context = SerializedProperty.GetValue<BotContext>();
		if ( _context == null )
			return;

		var blackboard = _context.Blackboard;

		int row = 0;
		foreach ( var kvp in blackboard )
		{
			var value = kvp.Value;

			_contentGrid.AddCell( 0, row, new Label( kvp.Key ) );
			_contentGrid.AddCell( 1, row, new Label( ToString( value ) ) );

			row++;
		}

		if ( blackboard.Count == 0 )
		{
			_contentGrid.AddCell( 0, 0, new Label( "No data available" ) );
			_contentGrid.AddCell( 1, 0, new Label( "" ) );
		}

		Update();
	}

	protected override void OnPaint()
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.Darken( 0.2f ) );
		Paint.DrawRect( LocalRect, 4 );
	}
}
