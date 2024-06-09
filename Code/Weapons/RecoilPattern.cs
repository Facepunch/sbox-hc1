using System.Text.Json.Serialization;

namespace Facepunch;

public struct RecoilPattern
{
	public RecoilPattern() { }

	public static Vector2 RangeX => new Vector2( -5, -5 );
	public static Vector2 RangeY => new Vector2( 0, 5 );

	public List<Vector2> Points { get; set; } = new();

	/// <summary>
	/// Should we be using loop points?
	/// </summary>
	[JsonIgnore]
	public bool UseLoopPoints => LoopStart != 0 && LoopEnd != 0;

	/// <summary>
	/// Which point index is our loop point start?
	/// </summary>
	public int LoopStart { get; set; }

	/// <summary>
	/// Which point index is our loop point start?
	/// </summary>
	public int LoopEnd { get; set; }

	/// <summary>
	/// Are we looping right now?
	/// </summary>
	public bool IsLooping { get; set; }

	[JsonIgnore]
	public int Count => Points.Count;

	public Vector2? FetchPoint( int index )
	{
		var pointCount = Points.Count;
		if ( index + 1 > pointCount ) return null;

		var point = Points[index];
		point.y = RangeY.y - point.y;

		return point;
	}

	/// <summary>
	/// Tries to get a point, and will wrap around if the index falls over.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public Vector2 GetPoint( ref int index )
	{
		var pointCount = Points.Count;

		var loopStart = LoopStart == 0 ? 0 : LoopStart;
		var loopEnd = LoopEnd == 0 ? 0 : pointCount - 1;

		// Wrap around.

		if ( index + 1 > pointCount || ( IsLooping && index == loopEnd ) )
		{
			// If we have a loop point, start at the loop point index and don't wrap around
			if ( loopStart != -1 )
			{

				IsLooping = true;
				index = loopStart;
				Log.Info( $"We're at {loopStart} because we started looping!" );
			}
			else
			{
				index = index % pointCount;
				Log.Info( $"We exceeded {pointCount}, so wrapping around to {index}" );
			}
		}

		var rawPoint = FetchPoint( index ) ?? default;
		var lastPoint = index - 1;
		Vector2 lastPointValue = lastPoint < 0 ? (FetchPoint( 0 ) ?? default) : (FetchPoint( lastPoint ) ?? default);
		return index == 0 ? rawPoint : new Vector2( rawPoint.x - lastPointValue.x, rawPoint.y - lastPointValue.y );
	}
}
