using Editor;
using Sandbox;

namespace Facepunch.Editor;

[EditorTool( "sound.sound-tool" )]
[Title( "Sound Tool" )]
[Icon( "mic" )]
[Alias( "soundtool" )]
[Group( "2" )]
file class SoundTool : EditorTool
{
	public static SoundTool Instance { get; set; }


	public SoundEvent SoundAsset { get; set; }
	public Vector3 WorldPosition { get; set; }
	
	MyWindow Window { get; set; }

	public override void OnEnabled()
	{
		SoundAsset = ResourceLibrary.Get<SoundEvent>( ProjectCookie.Get<string>( "soundtool.sound", null ) );
		Instance = this;
		Window = new MyWindow();
		AddOverlay( Window, TextFlag.RightBottom, 10 );
	}

	public void Play()
	{
		ProjectCookie.Set( "soundtool.sound", SoundAsset.ResourcePath );
		Sound.Play( SoundAsset, WorldPosition );
	}

	public override void OnUpdate()
	{
		Gizmo.Draw.Color = Color.Red.WithAlpha( 0.85f );
		Gizmo.Draw.SolidSphere( WorldPosition, 8f );

		Gizmo.Draw.Color = Color.Red.WithAlpha( 0.5f );
		Gizmo.Draw.LineSphere( WorldPosition, 32f );

		if ( Gizmo.WasLeftMousePressed )
		{
			var tr = Trace.UseRenderMeshes( true )
				.UsePhysicsWorld( true )
				.Run();

			WorldPosition = tr.EndPosition;
		}
	}
}

file class MyWindow : WidgetWindow
{
	public MyWindow()
	{
		ContentMargins = 0;
		Layout = Layout.Column();
		FixedSize = new( 400, 96 );

		Rebuild();
	}

	void Rebuild()
	{
		Layout.Clear( true );
		Layout.Margin = 0;
		Icon = "mic";
		WindowTitle = "Sound Tester";
		IsGrabbable = true;

		Layout.AddSpacingCell( 20f );
		Layout.Add( new Label( "Click a location in the world to set the sound's origin." ) );

		var sheet = Layout.Add( new PropertySheet( this ) );
		sheet.AddProperty( SoundTool.Instance, nameof( SoundTool.SoundAsset ) );

		Layout.Add( new Button( "Play Sound", "mic", this ) { MouseClick = SoundTool.Instance.Play } );
		Layout.Margin = 4;
	}
}
