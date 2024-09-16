public static class MathExtensions
{
	public static float MoveToLinear( this float from, float target, float speed )
	{
		var diff = target - from;
		var maxDelta = speed * Time.Delta;

		if ( Math.Abs( diff ) < maxDelta )
		{
			return target;
		}

		return from + Math.Sign( diff ) * maxDelta;
	}
}
