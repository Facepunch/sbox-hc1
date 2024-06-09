using Editor;
using Sandbox;
using System.Diagnostics;

namespace Facepunch.Editor;

public partial class RecoilPatternKey : GraphicsItem
{
	RecoilPatternInstance Instance => Parent as RecoilPatternInstance;

	public bool IsLoopStart => Instance.LoopStart == GetIndex();
	public bool IsLoopEnd => Instance.LoopEnd == GetIndex();

	/// <summary>
	/// Gets the index of this key from the recoil pattern instance
	/// </summary>
	/// <returns></returns>
	public int GetIndex()
	{
		if ( Instance is not null )
		{
			return Instance.Keys.FindIndex( x => x == this );
		}

		return 1;
	}

	public Vector2 Evaluate( Vector2 rangeX, Vector2 rangeY )
	{
		var x = Position.x / Instance.Size.x;
		var y = Position.y / Instance.Size.y;

		var remappedX = x.Remap( 0, 1, rangeX.x, rangeX.y );
		var remappedY = y.Remap( 0, 1, rangeY.x, rangeY.y );

		return new Vector2( remappedX, remappedY );
	}

	public RecoilPatternKey( GraphicsItem parent, Vector2 key ) : base( parent )
	{
		HoverEvents = true;
		Clip = false;
		Cursor = CursorShape.DragCopy;
		Movable = true;
		//Selectable = true;
		Size = new( 16.0f, 16.0f );
		Position = key;
	}

	protected override void OnPaint()
	{
		var rect = LocalRect;
		var mainColor = Hovered ? Theme.Selection : Color.Gray;

		var loopColor = Theme.Blue;
		if ( IsLoopEnd ) loopColor = Theme.Red;
		if ( Hovered ) mainColor = mainColor.Lighten( 0.2f );

		if ( IsLoopStart || IsLoopEnd )
		{
			Paint.ClearPen();
			Paint.SetBrush( loopColor.WithAlpha( 0.2f ) );
			Paint.DrawRect( rect );
		}

		Paint.SetPen( mainColor.Lighten( 0.2f ) );
		Paint.DrawIcon( rect, "close", 16, Sandbox.TextFlag.LeftCenter );
	}

	protected void OpenContextMenu()
	{
		var menu = new Menu( Instance.Editor );

		var loop = menu.AddMenu( "Loop Points", "all_inclusive" );
		loop.AddOption( "Mark as Start", null, () =>
		{
			Instance.LoopStart = GetIndex();
			Instance.Update();
		} );
		loop.AddOption( "Mark as End", null, () =>
		{
			Instance.LoopEnd = GetIndex();
			Instance.Update();
		} );
		menu.AddOption( "Delete", "trash", () => Instance.RemoveKey( this ) );

		menu.OpenAtCursor();
	}

	protected override void OnMousePressed( GraphicsMouseEvent e )
	{
		base.OnMousePressed( e );

		if ( e.RightMouseButton )
		{
			OpenContextMenu();
		}
	}

	protected override void OnMoved()
	{
		Instance.Value = Instance.ToPattern();

		Instance.Update();
	}
}
