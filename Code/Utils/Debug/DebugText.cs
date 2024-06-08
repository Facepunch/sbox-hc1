namespace Facepunch;

public partial class DebugText
{
	[ConVar( "hc1_debug" )] public static bool Enabled { get; set; }

	static int RowId = 0;

	static Vector2 Position
	{
		get
		{
			var pos = new Vector2( 16, Screen.Size.y / 2 );
			var rowHeight = 20;

			return pos + new Vector2( 0, rowHeight * RowId );
		}
	}

	public static void Spacer()
	{
		RowId++;
	}

	public static void Write( string text, Color color = default, int fontSize = 15 )
	{
		if ( !Enabled ) return;

		if ( color.a == 0 )
			color = Color.White;

		Gizmo.Draw.Color = color;
		Gizmo.Draw.ScreenText( text, Position, "Roboto", fontSize, TextFlag.Left );

		RowId++;
	}

	public static void Update()
	{
		RowId = 0;
	}
}
