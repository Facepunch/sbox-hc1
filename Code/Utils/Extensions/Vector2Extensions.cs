public static class Vector2Extensions
{
	/// <summary>
	/// Gets the angle (in degrees) between two Vector2s
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static float Angle( this Vector2 a, Vector2 b )
	{
		return (float)Math.Atan2( b.y - a.y, b.x - a.x ) * (180f / MathF.PI);
	}
}
