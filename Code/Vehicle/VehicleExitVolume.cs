namespace Facepunch;

public class VehicleExitVolume : Component
{
	[Property] public Vector3 Size { get; set; }


	protected override void DrawGizmos()
	{
		if ( !Gizmo.IsSelected )
			return;

		Gizmo.Draw.IgnoreDepth = true;
		Gizmo.Draw.Color = Color.White;
		Gizmo.Draw.LineThickness = 2.0f;

		var mins = -Size / 2f;
		var maxs = Size / 2f;
		Gizmo.Draw.LineBBox( new BBox( mins, maxs ) );
	}

	public Vector3 CheckClosestFreeSpace( Vector3 position )
	{
		var playerSize = new Vector3( 64, 64, 72 ); // Player bbox
		var maxDistance = 200f;
		var step = 16f;

		if ( IsSpaceFree( position, position, playerSize ) )
			return position;

		for ( float distance = step; distance <= maxDistance; distance += step )
		{
			for ( float x = -distance; x <= distance; x += step )
			{
				for ( float y = -distance; y <= distance; y += step )
				{
					for ( float z = -distance; z <= distance; z += step )
					{
						// Only check on the surface of the cube
						if ( Math.Abs( x ) != distance && Math.Abs( y ) != distance && Math.Abs( z ) != distance )
							continue;

						Vector3 testPos = position + new Vector3( x, y, z );
						if ( IsSpaceFree( position, testPos, playerSize ) )
						{
							return testPos;
						}
					}
				}
			}
		}

		// If no free space is found, return the original position
		// TODO: Return nothing and use fallback exit volume instead
		return position;
	}

	private bool IsSpaceFree( Vector3 from, Vector3 to, Vector3 size )
	{
		// Create a bounding box for the player
		BBox playerBox = new BBox( -size / 2, size / 2 );

		// Check for collisions
		var trace = Scene.Trace.Box( playerBox, to, to )
			.WithoutTags( "player", "wheel" )
			.Run();

		return !trace.Hit;
	}
}
