using System;
using Editor;
using Editor.GraphicsItems;
using Sandbox;
using static Editor.GraphicsItems.ChartBackground;

namespace Facepunch.Editor;

/// <summary>
/// A widget which contains an editable curve
/// </summary>
public class RecoilPatternEditor : GraphicsView
{
	ChartBackground Background;

	public RecoilPatternEditor( Widget parent ) : base( parent )
	{
		SceneRect = new( 0, Size );
		HorizontalScrollbar = ScrollbarMode.Off;
		VerticalScrollbar = ScrollbarMode.Off;
		Scale = 1;
		CenterOn( new Vector2( 100, 10 ) );

		Background = new ChartBackground
		{
			Size = SceneRect.Size,
			RangeX = new Vector2( -5, 5 ),
			RangeY = new Vector2( 0, 5 ),
			AxisX = new AxisConfig { LineColor = Theme.Text.WithAlpha( 0.2f ), Ticks = 11, Width = 30.0f, LabelColor = Theme.Text.WithAlpha( 0.5f ) },
			AxisY = new AxisConfig { LineColor = Theme.Text.WithAlpha( 0.2f ), Ticks = 11, Width = 20.0f, LabelColor = Theme.Text.WithAlpha( 0.5f ) }
		};
		Add( Background );
	}

	public void SetProperty( SerializedProperty property )
	{
		var instance = new RecoilPatternInstance( this )
		{
			RangeX = Background.RangeX,
			RangeY = Background.RangeY,
			Property = property
		};

		Add( instance );
	}

	protected override void DoLayout()
	{
		base.DoLayout();

		SceneRect = new( 0, Size );
		Background.Size = SceneRect.Size;
		
		foreach ( var i in Items )
		{
			if ( i is RecoilPatternInstance instance )
			{
				if ( instance.SceneRect == Background.ChartRect ) continue;
				instance.SceneRect = Background.ChartRect;
				instance.Refresh();
			}
		}
	}
}
