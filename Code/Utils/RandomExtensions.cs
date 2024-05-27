public static class RandomExtensions
{
	public static T FromList<T>( this Random random, IReadOnlyList<T> list )
	{
		if ( list.Count == 0 )
		{
			return default;
		}

		var index = random.Next( list.Count );
		return list[index];
	}

	public static IReadOnlyList<T> Shuffle<T>( this IEnumerable<T> items, Random random = null )
	{
		var copy = items.ToArray();

		random ??= Random.Shared;

		for ( var i = 0; i < copy.Length - 1; ++i )
		{
			var j = random.Next( i, copy.Length );

			(copy[i], copy[j]) = (copy[j], copy[i]);
		}

		return copy;
	}
}
