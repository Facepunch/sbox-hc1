using Editor;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Editor;

public partial class RecoilPatternInstance : GraphicsItem
{
	public Vector2 RangeX { get; init; }
	public Vector2 RangeY { get; init; }
	public RecoilPatternEditor Editor { get; init; }
	public SerializedProperty Property { get; init; }
	public List<RecoilPatternKey> Keys { get; private set; } = new();

	public RecoilPatternInstance( RecoilPatternEditor editor ) : base( null )
	{
		HoverEvents = true;
		Clip = true;
		Editor = editor;
	}

	public RecoilPattern Value
	{
		get => Property.GetValue<RecoilPattern>( new() );
		set => Property.SetValue( value );
	}
	public int LoopEnd
	{
		get => Value.LoopEnd;
		set=> Value = Value with { LoopEnd = value };
	}

	public int LoopStart
	{
		get => Value.LoopStart;
		set => Value = Value with { LoopStart = value };
	}

	internal void Refresh()
	{
		if ( Value.Points is null ) return;

		foreach ( var key in Keys )
		{
			key.Destroy();
		}

		Keys.Clear();

		foreach ( var point in Value.Points )
		{
			AddKey( GetRemappedPoint( point ) );
		}

		Update();
	}

	protected override void OnPaint()
	{
		var offset = new Vector2( 8.0f, 8.0f );

		Paint.SetPen( Color.Gray.WithAlpha( 0.5f ), 2, PenStyle.Dot );
		Paint.DrawLine( Keys.Select( x => x.Position + offset ) );

		if ( Value.LoopEnd != 0 && Value.LoopStart != 0 )
		{
			var startKey = Keys[Value.LoopStart];
			var endKey = Keys[Value.LoopEnd];

			Paint.SetPen( Theme.Red.WithAlpha( 0.5f ), 2, PenStyle.Dot );

			Paint.DrawLine( new List<Vector2> { startKey.Position + offset - new Vector2( 1, 1 ), endKey.Position + offset - new Vector2( 1, 1 ) } );
			Paint.SetPen( Theme.Blue.WithAlpha( 0.5f ), 2, PenStyle.Dot );
			Paint.DrawLine( new List<Vector2> { startKey.Position + offset + new Vector2( 1, 1 ), endKey.Position + offset + new Vector2( 1, 1 ) } );
		}
	}

	Vector2 GetRemappedPoint( Vector2 point )
	{
		var x = point.x;
		var y = point.y;

		var remappedX = x.Remap( RangeX.x, RangeX.y, 0, 1 );
		var remappedY = y.Remap( RangeY.x, RangeY.y, 0, 1 );

		return new Vector2( remappedX * Size.x, remappedY * Size.y ); ;
	}

	internal RecoilPattern ToPattern()
	{
		var pattern = new RecoilPattern()
		{
			Points = Keys.Select( x => x.Evaluate( RangeX, RangeY ) ).ToList(),
			LoopStart = LoopStart,
			LoopEnd = LoopEnd
		};

		return pattern;
	}

	internal void AddKey( Vector2 pos )
	{
		Keys.Add( new RecoilPatternKey( this, pos ) );
		Update();

		Value = ToPattern();
	}

	internal void RemoveKey( RecoilPatternKey key )
	{
		Keys.Remove( key );
		key.Destroy();
		Update();

		Value = ToPattern();
	}

	protected override void OnMousePressed( GraphicsMouseEvent e )
	{
		if ( e.LeftMouseButton )
		{
			// Are we selecting something?
			if ( Keys.FirstOrDefault( x => x.Hovered ) is RecoilPatternKey key )
				return;

			AddKey( e.LocalPosition );
		}
	}
}
