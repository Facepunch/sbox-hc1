using Editor;
using Sandbox;

namespace Facepunch.Editor;

public class MinimapRendererTool : EditorTool<MinimapRenderer>
{
	MinimapToolWindow window;

	public override void OnEnabled()
	{
		window = new MinimapToolWindow();
		AddOverlay( window, TextFlag.LeftBottom, 10 );
	}

	public override void OnUpdate()
	{
		window.ToolUpdate();
	}

	public override void OnSelectionChanged()
	{
		var renderer = GetSelectedComponent<MinimapRenderer>();
		if ( renderer.IsValid() )
		{
			window.OnSelectionChanged( renderer );
		}
	}
}


class MinimapToolWindow : WidgetWindow
{
	MinimapRenderer Renderer;

	SceneWidget SceneWidget;

	public MinimapToolWindow()
	{
		Icon = "photo_camera";
		ContentMargins = 0;
		Layout = Layout.Column();
		WindowTitle = "Minimap Generator";

		SceneWidget = new SceneWidget( this );
		SceneWidget.FixedWidth = 720 * 0.4f;
		SceneWidget.FixedHeight = 720 * 0.4f;

		var headerRow = Layout.AddRow();
		headerRow.AddStretchCell();
		headerRow.Add( new IconButton( "save", SaveToFile ) { ToolTip = "Save", FixedHeight = HeaderHeight, FixedWidth = HeaderHeight, Background = Color.Transparent });

		Layout.Add( SceneWidget );
		Layout.Margin = 4;
	}

	public void ToolUpdate()
	{
		if ( Renderer is null ) return;

		Renderer.Camera.UpdateSceneCamera( SceneWidget.Camera );
		SceneWidget.Camera.Rect = new Rect( 0, 0, 1, 1 );
	}

	internal void OnSelectionChanged( MinimapRenderer renderer )
	{
		Renderer = renderer;
		SceneWidget.Scene = renderer.Scene;
	}

	internal void SaveToFile()
	{
		var svf = EditorUtility.SaveFileDialog( $"Save Minimap File As..", "png", Project.Current.GetAssetsPath() );

		if ( svf != null )
		{
			var pixelMap = new Pixmap( Renderer.ExportResolution.x.CeilToInt(), Renderer.ExportResolution.y.CeilToInt() );
			SceneWidget.Camera.RenderToPixmap( pixelMap );
			pixelMap.SavePng( svf );
		}
	}
}

class SceneWidget : Widget
{
	public Scene Scene { get; set; }
	public SceneCamera Camera { get; set; } = new SceneCamera();

	Pixmap pixmap;

	public SceneWidget( Widget parent ) : base( parent )
	{
		//
	}

	[EditorEvent.Frame]
	internal void Frame()
	{
		if ( !Visible ) return;

		var realSize = Size * DpiScale;

		if ( pixmap is null || pixmap.Size != realSize )
		{
			pixmap = new Pixmap( realSize );
		}

		if ( Scene is not null )
		{
			Camera.World = Scene.SceneWorld;
			Camera.Worlds.Clear();

			Camera.RenderToPixmap( pixmap );
		}

		Update();
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		if ( pixmap is not null )
		{
			Paint.Draw( LocalRect, pixmap );
		}
	}
}
